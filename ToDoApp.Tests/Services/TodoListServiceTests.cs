using Microsoft.EntityFrameworkCore;
using ToDoApp.Data;
using ToDoApp.Models;
using ToDoApp.Services;

namespace ToDoApp.Tests.Services
{
    public class TodoListServiceTests
    {
        private static TodoListService CreateService(out AppDbContext db)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            db = new AppDbContext(options);
            // El seed de HasData (lista "Mis Tareas") solo se materializa al crear
            // el esquema; con InMemory eso requiere EnsureCreated explícito (no hay
            // Migrate() para este proveedor).
            db.Database.EnsureCreated();
            return new TodoListService(db);
        }

        [Fact]
        public async Task GetAllAsync_IncludesSeededDefaultList()
        {
            var service = CreateService(out _);

            var lists = (await service.GetAllAsync()).ToList();

            Assert.Single(lists);
            Assert.Equal(TodoList.NombrePredeterminada, lists[0].Nombre);
            Assert.True(lists[0].EsPredeterminada);
        }

        [Fact]
        public async Task AddAsync_AppendsList_AtTheEnd()
        {
            var service = CreateService(out _);

            var lista = await service.AddAsync("Programación");

            Assert.True(lista.Id > 0);
            Assert.Equal(1, lista.Orden); // 0 = Mis Tareas (seed), 1 = la nueva
            Assert.False(lista.EsPredeterminada);
        }

        [Fact]
        public async Task RenameAsync_PersistsNewName()
        {
            var service = CreateService(out var db);
            var lista = await service.AddAsync("Original");

            await service.RenameAsync(lista.Id, "Renombrada");

            var reloaded = await db.TodoLists.FindAsync(lista.Id);
            Assert.Equal("Renombrada", reloaded!.Nombre);
        }

        [Fact]
        public async Task DeleteAsync_OnDefaultList_Throws()
        {
            var service = CreateService(out var db);
            var defaultList = await db.TodoLists.FirstAsync(l => l.EsPredeterminada);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.DeleteAsync(defaultList.Id, moverTareasAMisTareas: false));
        }

        [Fact]
        public async Task DeleteAsync_MovingTasks_ReassignsToDefaultList_ThenDeletesList()
        {
            var service = CreateService(out var db);
            var lista = await service.AddAsync("Temporal");
            var todoService = new TodoService(db);
            var item = await todoService.AddAsync(new TodoItem { Title = "Sobrevive", TodoListId = lista.Id });

            await service.DeleteAsync(lista.Id, moverTareasAMisTareas: true);

            var reloadedItem = await db.TodoItems.FindAsync(item.Id);
            var defaultList = await db.TodoLists.FirstAsync(l => l.EsPredeterminada);
            Assert.Equal(defaultList.Id, reloadedItem!.TodoListId);
            Assert.Null(await db.TodoLists.FindAsync(lista.Id));
        }

        [Fact]
        public async Task DeleteAsync_NotMovingTasks_DeletesTasksToo()
        {
            var service = CreateService(out var db);
            var lista = await service.AddAsync("Temporal");
            var todoService = new TodoService(db);
            await todoService.AddAsync(new TodoItem { Title = "No sobrevive", TodoListId = lista.Id });

            await service.DeleteAsync(lista.Id, moverTareasAMisTareas: false);

            Assert.Empty(await db.TodoItems.ToListAsync());
            Assert.Null(await db.TodoLists.FindAsync(lista.Id));
        }

        [Fact]
        public async Task DeleteAsync_NonExistentId_DoesNotThrow()
        {
            var service = CreateService(out _);

            var exception = await Record.ExceptionAsync(() => service.DeleteAsync(999, moverTareasAMisTareas: false));

            Assert.Null(exception);
        }
    }
}
