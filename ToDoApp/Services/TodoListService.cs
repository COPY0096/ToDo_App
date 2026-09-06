using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ToDoApp.Data;
using ToDoApp.Models;

namespace ToDoApp.Services
{
    public class TodoListService
    {
        private readonly AppDbContext _db;
        public TodoListService(AppDbContext db) { _db = db; }

        public async Task<IEnumerable<TodoList>> GetAllAsync() =>
            await _db.TodoLists.OrderBy(l => l.Orden).ToListAsync();

        public async Task<TodoList> AddAsync(string nombre)
        {
            var maxOrden = await _db.TodoLists.AnyAsync() ? await _db.TodoLists.MaxAsync(l => l.Orden) : -1;
            var list = new TodoList { Nombre = nombre, Orden = maxOrden + 1 };
            _db.TodoLists.Add(list);
            await _db.SaveChangesAsync();
            return list;
        }

        public async Task RenameAsync(int id, string nombre)
        {
            var list = await _db.TodoLists.FindAsync(id);
            if (list is null) return;
            list.Nombre = nombre;
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Elimina una lista no predeterminada. Si <paramref name="moverTareasAMisTareas"/> es
        /// true, sus tareas se reasignan a la lista predeterminada antes de borrarla; si es
        /// false, las tareas se eliminan junto con la lista. La decisión la pregunta la UI en
        /// cada caso (no hay un comportamiento fijo).
        /// </summary>
        /// <exception cref="InvalidOperationException">Si se intenta borrar la lista predeterminada.</exception>
        public async Task DeleteAsync(int id, bool moverTareasAMisTareas)
        {
            var list = await _db.TodoLists.Include(l => l.Items).FirstOrDefaultAsync(l => l.Id == id);
            if (list is null) return;

            if (list.EsPredeterminada)
                throw new InvalidOperationException("No se puede eliminar la lista predeterminada.");

            if (moverTareasAMisTareas)
            {
                var defaultList = await _db.TodoLists.FirstOrDefaultAsync(l => l.EsPredeterminada);
                if (defaultList is null)
                    throw new InvalidOperationException("No se encontró la lista predeterminada.");

                foreach (var item in list.Items)
                    item.TodoListId = defaultList.Id;
            }
            else
            {
                _db.TodoItems.RemoveRange(list.Items);
            }

            _db.TodoLists.Remove(list);
            await _db.SaveChangesAsync();
        }
    }
}
