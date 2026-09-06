using Microsoft.EntityFrameworkCore;
using ToDoApp.Data;
using ToDoApp.Models;
using ToDoApp.Services;
using ToDoApp.ViewModels;

namespace ToDoApp.Tests.ViewModels
{
    public class MainViewModelTests
    {
        private static MainViewModel CreateViewModel(out TodoService todoService, out TodoListService todoListService)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var db = new AppDbContext(options);
            // El seed de HasData (lista "Mis Tareas") solo se materializa al crear
            // el esquema; con InMemory eso requiere EnsureCreated explícito (no hay
            // Migrate() para este proveedor).
            db.Database.EnsureCreated();
            todoService = new TodoService(db);
            todoListService = new TodoListService(db);
            return new MainViewModel(todoService, todoListService);
        }

        [Fact]
        public async Task InitializeAsync_CreatesDefaultListColumn_FromSeedData()
        {
            var vm = CreateViewModel(out _, out _);

            await vm.InitializeAsync();

            Assert.Single(vm.Lists);
            Assert.Equal(TodoList.NombrePredeterminada, vm.Lists[0].Nombre);
            Assert.True(vm.Lists[0].EsPredeterminada);
        }

        [Fact]
        public async Task InitializeAsync_GroupsExistingItems_IntoTheirOwnList()
        {
            var vm = CreateViewModel(out var todoService, out var todoListService);
            var otraLista = await todoListService.AddAsync("Programación");
            await todoService.AddAsync(new TodoItem { Title = "En Mis Tareas", TodoListId = 1 });
            await todoService.AddAsync(new TodoItem { Title = "En Programación", TodoListId = otraLista.Id });

            await vm.InitializeAsync();

            var misTareas = vm.Lists.Single(l => l.EsPredeterminada);
            var programacion = vm.Lists.Single(l => l.Id == otraLista.Id);
            Assert.Single(misTareas.Items);
            Assert.Equal("En Mis Tareas", misTareas.Items[0].Title);
            Assert.Single(programacion.Items);
            Assert.Equal("En Programación", programacion.Items[0].Title);
        }

        [Fact]
        public async Task InitializeAsync_PutsCompletedItems_InCompletedBucket()
        {
            var vm = CreateViewModel(out var todoService, out _);
            await todoService.AddAsync(new TodoItem { Title = "Hecha", TodoListId = 1, Estado = TodoEstado.Completado });

            await vm.InitializeAsync();

            var column = vm.Lists.Single();
            Assert.Empty(column.Items);
            Assert.Single(column.CompletedItems);
        }

        [Fact]
        public async Task AddList_CreatesNewColumn_AndPersists()
        {
            var vm = CreateViewModel(out _, out var todoListService);
            await vm.InitializeAsync();

            vm.NewListName = "Ayuntamiento";
            vm.AddListCommand.Execute(null);
            await Task.Delay(50);

            Assert.Equal(2, vm.Lists.Count);
            Assert.Contains(vm.Lists, l => l.Nombre == "Ayuntamiento");
            Assert.Equal(string.Empty, vm.NewListName);
            Assert.Contains(await todoListService.GetAllAsync(), l => l.Nombre == "Ayuntamiento");
        }

        [Fact]
        public void AddListCommand_CanExecute_IsFalse_WhenNameIsEmpty()
        {
            var vm = CreateViewModel(out _, out _);

            Assert.False(vm.AddListCommand.CanExecute(null));
        }

        [Fact]
        public async Task ColumnAddTask_AddsToItems_PersistsWithCorrectList()
        {
            var vm = CreateViewModel(out var todoService, out _);
            await vm.InitializeAsync();
            var column = vm.Lists.Single();

            column.NewTaskTitle = "Comprar leche";
            column.AddTaskCommand.Execute(null);
            await Task.Delay(50);

            Assert.Single(column.Items);
            var persisted = (await todoService.GetAllAsync()).Single();
            Assert.Equal("Comprar leche", persisted.Title);
            Assert.Equal(column.Id, persisted.TodoListId);
            Assert.Equal(string.Empty, column.NewTaskTitle);
        }

        [Fact]
        public async Task ColumnItem_TogglingEstadoCompletado_MovesToCompletedBucket_AndPersists()
        {
            var vm = CreateViewModel(out var todoService, out _);
            await vm.InitializeAsync();
            var column = vm.Lists.Single();
            column.NewTaskTitle = "Marcar completa";
            column.AddTaskCommand.Execute(null);
            await Task.Delay(50);
            var item = column.Items[0];

            item.Estado = TodoEstado.Completado;
            await Task.Delay(50);

            Assert.Empty(column.Items);
            Assert.Single(column.CompletedItems);
            var persisted = (await todoService.GetAllAsync()).Single();
            Assert.True(persisted.IsDone);
        }

        [Fact]
        public async Task ColumnDeleteTask_RemovesFromItems_AndPersistence()
        {
            var vm = CreateViewModel(out var todoService, out _);
            await vm.InitializeAsync();
            var column = vm.Lists.Single();
            column.NewTaskTitle = "Para borrar";
            column.AddTaskCommand.Execute(null);
            await Task.Delay(50);
            var item = column.Items[0];

            column.DeleteTaskCommand.Execute(item);
            await Task.Delay(50);

            Assert.Empty(column.Items);
            Assert.Empty(await todoService.GetAllAsync());
        }

        [Fact]
        public async Task DeleteListAsync_OnDefaultList_DoesNothing()
        {
            var vm = CreateViewModel(out _, out _);
            await vm.InitializeAsync();
            var defaultColumn = vm.Lists.Single();

            await vm.DeleteListAsync(defaultColumn, moverTareasAMisTareas: false);

            Assert.Single(vm.Lists);
        }

        [Fact]
        public async Task DeleteListAsync_MovingTasks_ReassignsThemToDefaultList()
        {
            var vm = CreateViewModel(out var todoService, out _);
            await vm.InitializeAsync();
            vm.NewListName = "Temporal";
            vm.AddListCommand.Execute(null);
            await Task.Delay(50);
            var columna = vm.Lists.Single(l => l.Nombre == "Temporal");
            columna.NewTaskTitle = "Sobrevive";
            columna.AddTaskCommand.Execute(null);
            await Task.Delay(50);

            await vm.DeleteListAsync(columna, moverTareasAMisTareas: true);

            Assert.Single(vm.Lists); // solo queda "Mis Tareas"
            var misTareas = vm.Lists.Single();
            Assert.Single(misTareas.Items);
            Assert.Equal("Sobrevive", misTareas.Items[0].Title);
            var persisted = (await todoService.GetAllAsync()).Single();
            Assert.Equal(misTareas.Id, persisted.TodoListId);
        }

        [Fact]
        public async Task DeleteListAsync_DeletingTasks_RemovesThemPermanently()
        {
            var vm = CreateViewModel(out var todoService, out _);
            await vm.InitializeAsync();
            vm.NewListName = "Temporal";
            vm.AddListCommand.Execute(null);
            await Task.Delay(50);
            var columna = vm.Lists.Single(l => l.Nombre == "Temporal");
            columna.NewTaskTitle = "No sobrevive";
            columna.AddTaskCommand.Execute(null);
            await Task.Delay(50);

            await vm.DeleteListAsync(columna, moverTareasAMisTareas: false);

            Assert.Single(vm.Lists);
            Assert.Empty(await todoService.GetAllAsync());
        }
    }
}
