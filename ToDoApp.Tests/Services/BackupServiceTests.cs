using Microsoft.EntityFrameworkCore;
using ToDoApp.Data;
using ToDoApp.Models;
using ToDoApp.Services;

namespace ToDoApp.Tests.Services
{
    public class BackupServiceTests
    {
        private static (BackupService backup, TodoService todo, TodoListService lists, AppDbContext db) CreateServices()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var db = new AppDbContext(options);
            db.Database.EnsureCreated(); // materializa el seed de "Mis Tareas" (HasData)
            return (new BackupService(db), new TodoService(db), new TodoListService(db), db);
        }

        [Fact]
        public async Task ExportAsync_IncludesSeededDefaultList_EvenWithNoTasks()
        {
            var (backup, _, _, _) = CreateServices();

            var data = await backup.ExportAsync();

            Assert.Single(data.Lists);
            Assert.Equal(TodoList.NombrePredeterminada, data.Lists[0].Nombre);
            Assert.True(data.Lists[0].EsPredeterminada);
            Assert.Empty(data.Lists[0].Tareas);
        }

        [Fact]
        public async Task ExportAsync_NestsSubTasksUnderTheirParent_NotAsTopLevelEntries()
        {
            var (backup, todo, _, _) = CreateServices();
            var parent = await todo.AddAsync(new TodoItem { Title = "Padre", TodoListId = 1 });
            await todo.AddSubTaskAsync(new TodoItem { Title = "Hija" }, parent.Id);

            var data = await backup.ExportAsync();

            var list = data.Lists.Single();
            var padre = Assert.Single(list.Tareas);
            Assert.Equal("Padre", padre.Title);
            var hija = Assert.Single(padre.SubTareas);
            Assert.Equal("Hija", hija.Title);
        }

        [Fact]
        public async Task ImportAsync_ReplacesExistingData()
        {
            var (backup, todo, lists, db) = CreateServices();
            await todo.AddAsync(new TodoItem { Title = "Se va a borrar", TodoListId = 1 });

            var data = new BackupData
            {
                ExportedAt = DateTime.Now,
                Lists =
                {
                    new TodoListBackup
                    {
                        Nombre = "Restaurada",
                        Orden = 0,
                        EsPredeterminada = true,
                        Tareas = { new TodoItemBackup { Title = "Sobrevive", Estado = TodoEstado.Pendiente, FechaCreacion = DateTime.Now } }
                    }
                }
            };

            await backup.ImportAsync(data);

            var todosLosItems = await db.TodoItems.ToListAsync();
            var todasLasListas = await lists.GetAllAsync();
            Assert.Single(todasLasListas);
            Assert.Equal("Restaurada", todasLasListas.Single().Nombre);
            Assert.Single(todosLosItems);
            Assert.Equal("Sobrevive", todosLosItems[0].Title);
        }

        [Fact]
        public async Task ImportAsync_RestoresHierarchy_WithFreshIds()
        {
            var (backup, _, _, db) = CreateServices();
            var data = new BackupData
            {
                Lists =
                {
                    new TodoListBackup
                    {
                        Nombre = "Con subtareas",
                        EsPredeterminada = true,
                        Tareas =
                        {
                            new TodoItemBackup
                            {
                                Title = "Padre",
                                FechaCreacion = DateTime.Now,
                                SubTareas = { new TodoItemBackup { Title = "Hija", FechaCreacion = DateTime.Now } }
                            }
                        }
                    }
                }
            };

            await backup.ImportAsync(data);

            var padre = await db.TodoItems.Include(t => t.SubTasks).SingleAsync(t => t.Title == "Padre");
            var hija = Assert.Single(padre.SubTasks);
            Assert.Equal("Hija", hija.Title);
            Assert.Equal(padre.Id, hija.ParentTaskId);
            Assert.Equal(padre.TodoListId, hija.TodoListId);
        }

        [Fact]
        public async Task ImportAsync_IgnoresNestingBeyondOneLevel()
        {
            // Un archivo editado a mano podría traer subtareas-de-subtareas; el modelo
            // BackupData lo permite (List<TodoItemBackup> recursivo) pero la app solo
            // soporta un nivel (igual que TodoService.AddSubTaskAsync).
            var (backup, _, _, db) = CreateServices();
            var data = new BackupData
            {
                Lists =
                {
                    new TodoListBackup
                    {
                        Nombre = "Lista",
                        EsPredeterminada = true,
                        Tareas =
                        {
                            new TodoItemBackup
                            {
                                Title = "Nivel 1",
                                SubTareas =
                                {
                                    new TodoItemBackup
                                    {
                                        Title = "Nivel 2",
                                        SubTareas = { new TodoItemBackup { Title = "Nivel 3 (debe ignorarse)" } }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            await backup.ImportAsync(data);

            Assert.DoesNotContain(await db.TodoItems.ToListAsync(), t => t.Title == "Nivel 3 (debe ignorarse)");
            Assert.Contains(await db.TodoItems.ToListAsync(), t => t.Title == "Nivel 2");
        }

        [Fact]
        public async Task ImportAsync_WithNoDefaultListFlagged_PicksFirstAsDefault()
        {
            var (backup, _, lists, _) = CreateServices();
            var data = new BackupData
            {
                Lists =
                {
                    new TodoListBackup { Nombre = "Primera", EsPredeterminada = false },
                    new TodoListBackup { Nombre = "Segunda", EsPredeterminada = false }
                }
            };

            await backup.ImportAsync(data);

            var todasLasListas = (await lists.GetAllAsync()).ToList();
            Assert.Single(todasLasListas, l => l.EsPredeterminada);
            Assert.Equal("Primera", todasLasListas.Single(l => l.EsPredeterminada).Nombre);
        }

        [Fact]
        public async Task ImportAsync_WithNoLists_Throws()
        {
            var (backup, _, _, _) = CreateServices();

            await Assert.ThrowsAsync<InvalidOperationException>(() => backup.ImportAsync(new BackupData()));
        }

        [Fact]
        public async Task ExportThenImport_RoundTrip_PreservesData()
        {
            var (backup, todo, listsService, _) = CreateServices();
            var otraLista = await listsService.AddAsync("Programación");
            await todo.AddAsync(new TodoItem { Title = "Tarea 1", Description = "Desc 1", TodoListId = 1, FechaVencimiento = new DateTime(2026, 12, 1) });
            var conSub = await todo.AddAsync(new TodoItem { Title = "Con subtarea", TodoListId = otraLista.Id });
            await todo.AddSubTaskAsync(new TodoItem { Title = "Subtarea" }, conSub.Id);

            var exported = await backup.ExportAsync();

            // Importar en una base nueva y vacía (simula restaurar en otra instalación).
            var (freshBackup, _, freshLists, freshDb) = CreateServices();
            await freshBackup.ImportAsync(exported);

            var todasLasListas = (await freshLists.GetAllAsync()).ToList();
            Assert.Equal(2, todasLasListas.Count);
            Assert.Contains(todasLasListas, l => l.Nombre == "Programación");

            var tarea1 = await freshDb.TodoItems.SingleAsync(t => t.Title == "Tarea 1");
            Assert.Equal("Desc 1", tarea1.Description);
            Assert.Equal(new DateTime(2026, 12, 1), tarea1.FechaVencimiento);

            var subtarea = await freshDb.TodoItems.SingleAsync(t => t.Title == "Subtarea");
            var padre = await freshDb.TodoItems.SingleAsync(t => t.Title == "Con subtarea");
            Assert.Equal(padre.Id, subtarea.ParentTaskId);
            Assert.Equal(padre.TodoListId, subtarea.TodoListId);
        }
    }
}
