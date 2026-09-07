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
        public async Task InitializeAsync_BuildsSubTaskTree_AndKeepsSubTasksOutOfColumnItems()
        {
            var vm = CreateViewModel(out var todoService, out _);
            var parent = await todoService.AddAsync(new TodoItem { Title = "Padre", TodoListId = 1 });
            await todoService.AddSubTaskAsync(new TodoItem { Title = "Hija" }, parent.Id);

            await vm.InitializeAsync();

            var column = vm.Lists.Single();
            Assert.Single(column.Items); // la subtarea no cuenta como tarea propia de la columna
            Assert.Single(column.Items[0].SubTasks);
            Assert.Equal("Hija", column.Items[0].SubTasks[0].Title);
        }

        [Fact]
        public async Task ColumnAddSubTask_AddsToParentSubTasks_AndPersistsWithSameList()
        {
            var vm = CreateViewModel(out var todoService, out _);
            await vm.InitializeAsync();
            var column = vm.Lists.Single();
            column.NewTaskTitle = "Tarea con hijas";
            column.AddTaskCommand.Execute(null);
            await Task.Delay(50);
            var parent = column.Items[0];

            parent.NewSubTaskTitle = "Subtarea nueva";
            column.AddSubTaskCommand.Execute(parent);
            await Task.Delay(50);

            Assert.Single(parent.SubTasks);
            Assert.Equal(string.Empty, parent.NewSubTaskTitle);
            var persisted = (await todoService.GetSubTasksAsync(parent.Id)).Single();
            Assert.Equal("Subtarea nueva", persisted.Title);
            Assert.Equal(parent.TodoListId, persisted.TodoListId);
        }

        [Fact]
        public async Task ColumnDeleteSubTask_RemovesFromParent_AndPersistence()
        {
            var vm = CreateViewModel(out var todoService, out _);
            await vm.InitializeAsync();
            var column = vm.Lists.Single();
            column.NewTaskTitle = "Tarea con hijas";
            column.AddTaskCommand.Execute(null);
            await Task.Delay(50);
            var parent = column.Items[0];
            parent.NewSubTaskTitle = "Para borrar";
            column.AddSubTaskCommand.Execute(parent);
            await Task.Delay(50);
            var subTask = parent.SubTasks[0];

            column.DeleteSubTaskCommand.Execute(subTask);
            await Task.Delay(50);

            Assert.Empty(parent.SubTasks);
            Assert.Empty(await todoService.GetSubTasksAsync(parent.Id));
        }

        [Fact]
        public async Task MoveTaskAsync_MovesTaskAndSubTasks_BetweenColumns_AndPersists()
        {
            var vm = CreateViewModel(out var todoService, out var todoListService);
            await vm.InitializeAsync();
            var origen = vm.Lists.Single(l => l.EsPredeterminada);
            var destinoLista = await todoListService.AddAsync("Programación");
            var destino = new TodoListColumnViewModel(destinoLista, todoService, todoListService);
            vm.Lists.Add(destino);

            origen.NewTaskTitle = "Con subtarea";
            origen.AddTaskCommand.Execute(null);
            await Task.Delay(50);
            var task = origen.Items[0];
            task.NewSubTaskTitle = "Hija";
            origen.AddSubTaskCommand.Execute(task);
            await Task.Delay(50);
            var subTask = task.SubTasks[0];

            await vm.MoveTaskAsync(task, destino);

            Assert.Empty(origen.Items);
            Assert.Single(destino.Items);
            Assert.Same(task, destino.Items[0]);
            Assert.Single(destino.Items[0].SubTasks);

            var persistedTask = (await todoService.GetAllAsync()).Single(t => t.Id == task.Id);
            var persistedSubTask = (await todoService.GetAllAsync()).Single(t => t.Id == subTask.Id);
            Assert.Equal(destino.Id, persistedTask.TodoListId);
            Assert.Equal(destino.Id, persistedSubTask.TodoListId);
        }

        [Fact]
        public async Task MoveTaskAsync_NoOp_WhenTargetIsSameList()
        {
            var vm = CreateViewModel(out _, out _);
            await vm.InitializeAsync();
            var column = vm.Lists.Single();
            column.NewTaskTitle = "Se queda";
            column.AddTaskCommand.Execute(null);
            await Task.Delay(50);
            var task = column.Items[0];

            await vm.MoveTaskAsync(task, column);

            Assert.Single(column.Items);
            Assert.Same(task, column.Items[0]);
        }

        [Fact]
        public async Task MoveTaskAsync_NoOp_ForSubTask()
        {
            var vm = CreateViewModel(out var todoService, out var todoListService);
            await vm.InitializeAsync();
            var origen = vm.Lists.Single();
            var destino = new TodoListColumnViewModel(await todoListService.AddAsync("Otra"), todoService, todoListService);
            origen.NewTaskTitle = "Padre";
            origen.AddTaskCommand.Execute(null);
            await Task.Delay(50);
            var parent = origen.Items[0];
            parent.NewSubTaskTitle = "Hija";
            origen.AddSubTaskCommand.Execute(parent);
            await Task.Delay(50);
            var subTask = parent.SubTasks[0];

            await vm.MoveTaskAsync(subTask, destino);

            Assert.Single(parent.SubTasks); // no se movió a ningún lado
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
