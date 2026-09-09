using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ToDoApp.Data;
using ToDoApp.Models;

namespace ToDoApp.Services
{
    /// <summary>
    /// Backup/restore manual del board completo a un archivo (Sprint 4). Ver
    /// SPRINT4.md para las decisiones de diseño (import = reemplazo total, no merge).
    /// </summary>
    public class BackupService
    {
        private readonly AppDbContext _db;
        public BackupService(AppDbContext db) { _db = db; }

        /// <summary>
        /// Arma el árbol de backup leyendo explícitamente las relaciones (no depende
        /// del fixup automático de EF que usa el resto de la app — un backup tiene
        /// que ser correcto sin importar la vida útil del AppDbContext).
        /// </summary>
        public async Task<BackupData> ExportAsync()
        {
            var lists = await _db.TodoLists.OrderBy(l => l.Orden).ToListAsync();
            var backup = new BackupData { ExportedAt = DateTime.Now };

            foreach (var list in lists)
            {
                var topLevelItems = await _db.TodoItems
                    .Include(i => i.SubTasks)
                    .Where(i => i.TodoListId == list.Id && i.ParentTaskId == null)
                    .ToListAsync();

                backup.Lists.Add(new TodoListBackup
                {
                    Nombre = list.Nombre,
                    Orden = list.Orden,
                    EsPredeterminada = list.EsPredeterminada,
                    Tareas = topLevelItems.Select(ToBackup).ToList()
                });
            }

            return backup;
        }

        private static TodoItemBackup ToBackup(TodoItem item) => new TodoItemBackup
        {
            Title = item.Title,
            Description = item.Description,
            Estado = item.Estado,
            FechaCreacion = item.FechaCreacion,
            FechaVencimiento = item.FechaVencimiento,
            SubTareas = item.SubTasks.Select(ToBackup).ToList()
        };

        /// <summary>
        /// Reemplaza TODOS los datos actuales por los del backup, en una transacción.
        /// Si ninguna lista del backup viene marcada como predeterminada (archivo
        /// armado a mano, o corrupto), se marca la primera como predeterminada — la
        /// app no puede quedar sin una lista predeterminada.
        /// </summary>
        /// <exception cref="InvalidOperationException">Si el backup no trae ninguna lista.</exception>
        public async Task ImportAsync(BackupData backup)
        {
            if (backup.Lists.Count == 0)
                throw new InvalidOperationException("El backup no tiene ninguna lista; no se importó nada.");

            // El proveedor InMemory (usado en los tests) no soporta transacciones y
            // lanza si se le pide una; SQLite real sí. Sin transacción en InMemory no
            // hay riesgo real porque cada test usa una base descartable de todos modos.
            var transaction = _db.Database.IsRelational()
                ? await _db.Database.BeginTransactionAsync()
                : null;
            await using var _ = transaction;

            _db.TodoItems.RemoveRange(_db.TodoItems);
            _db.TodoLists.RemoveRange(_db.TodoLists);
            await _db.SaveChangesAsync();

            var hayPredeterminada = backup.Lists.Any(l => l.EsPredeterminada);

            foreach (var (listBackup, index) in backup.Lists.Select((l, i) => (l, i)))
            {
                var list = new TodoList
                {
                    Nombre = listBackup.Nombre,
                    Orden = listBackup.Orden,
                    EsPredeterminada = hayPredeterminada ? listBackup.EsPredeterminada : index == 0
                };
                _db.TodoLists.Add(list);
                await _db.SaveChangesAsync(); // asigna list.Id antes de usarlo como FK

                foreach (var taskBackup in listBackup.Tareas)
                    AddTask(taskBackup, list.Id, parentTaskId: null, isTopLevel: true);

                await _db.SaveChangesAsync();
            }

            if (transaction is not null)
                await transaction.CommitAsync();
        }

        private void AddTask(TodoItemBackup taskBackup, int listId, int? parentTaskId, bool isTopLevel)
        {
            var task = new TodoItem
            {
                Title = taskBackup.Title,
                Description = taskBackup.Description,
                Estado = taskBackup.Estado,
                FechaCreacion = taskBackup.FechaCreacion,
                FechaVencimiento = taskBackup.FechaVencimiento,
                TodoListId = listId,
                ParentTaskId = parentTaskId
            };
            _db.TodoItems.Add(task);
            _db.SaveChanges(); // asigna task.Id antes de usarlo como ParentTaskId de sus subtareas

            // Límite de un solo nivel (igual que TodoService.AddSubTaskAsync): si esta
            // tarea ya es una subtarea, sus propias SubTareas se ignoran en vez de
            // importarse — protege el invariante aunque el archivo venga editado a mano
            // con más niveles de los que la UI puede mostrar.
            if (isTopLevel)
            {
                foreach (var subTaskBackup in taskBackup.SubTareas)
                    AddTask(subTaskBackup, listId, parentTaskId: task.Id, isTopLevel: false);
            }
        }
    }
}
