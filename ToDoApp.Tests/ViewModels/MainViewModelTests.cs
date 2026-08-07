using Microsoft.EntityFrameworkCore;
using ToDoApp.Data;
using ToDoApp.Models;
using ToDoApp.Services;
using ToDoApp.ViewModels;

namespace ToDoApp.Tests.ViewModels
{
    public class MainViewModelTests
    {
        private static MainViewModel CreateViewModel(out TodoService service)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            service = new TodoService(new AppDbContext(options));
            return new MainViewModel(service);
        }

        [Fact]
        public async Task InitializeAsync_LoadsExistingItems_IntoItemsCollection()
        {
            var vm = CreateViewModel(out var service);
            await service.AddAsync(new TodoItem { Title = "Existente" });

            await vm.InitializeAsync();

            Assert.Single(vm.Items);
            Assert.Equal("Existente", vm.Items[0].Title);
        }

        [Fact]
        public void CanAdd_IsFalse_WhenNewTitleIsEmpty()
        {
            var vm = CreateViewModel(out _);

            Assert.False(vm.AddTaskCommand.CanExecute(null));
        }

        [Fact]
        public void CanAdd_IsTrue_WhenNewTitleIsSet()
        {
            var vm = CreateViewModel(out _);

            vm.NewTitle = "Nueva tarea";

            Assert.True(vm.AddTaskCommand.CanExecute(null));
        }

        [Fact]
        public void CanAdd_IsFalse_WhenNewTitleIsWhitespace()
        {
            var vm = CreateViewModel(out _);

            vm.NewTitle = "   ";

            Assert.False(vm.AddTaskCommand.CanExecute(null));
        }

        [Fact]
        public async Task AddItem_WithBlankTitle_DoesNotAddToItemsOrPersist()
        {
            var vm = CreateViewModel(out var service);
            vm.NewTitle = "   ";

            vm.AddTaskCommand.Execute(null);
            await Task.Delay(50); // AddItem is async void; let any (skipped) work settle.

            Assert.Empty(vm.Items);
            Assert.Empty(await service.GetAllAsync());
        }

        [Fact]
        public async Task AddItem_WithValidTitle_AddsToItems_PersistsAndClearsInputs()
        {
            var vm = CreateViewModel(out var service);
            vm.NewTitle = "Comprar leche";
            vm.NewDescription = "Descremada";

            vm.AddTaskCommand.Execute(null);
            await Task.Delay(50); // AddItem is async void; wait for the fire-and-forget save.

            Assert.Single(vm.Items);
            Assert.Equal("Comprar leche", vm.Items[0].Title);
            Assert.Equal(string.Empty, vm.NewTitle);
            Assert.Equal(string.Empty, vm.NewDescription);
            Assert.Single(await service.GetAllAsync());
        }

        [Fact]
        public async Task TogglingItemIsDone_PersistsChange()
        {
            var vm = CreateViewModel(out var service);
            vm.NewTitle = "Marcar completa";
            vm.AddTaskCommand.Execute(null);
            await Task.Delay(50);
            var item = vm.Items[0];

            item.IsDone = true;
            await Task.Delay(50); // Item_PropertyChanged persists asynchronously.

            var persisted = (await service.GetAllAsync()).Single();
            Assert.True(persisted.IsDone);
        }

        [Fact]
        public async Task DeleteItem_RemovesFromItems_AndPersistence()
        {
            var vm = CreateViewModel(out var service);
            vm.NewTitle = "Para borrar";
            vm.AddTaskCommand.Execute(null);
            await Task.Delay(50);
            var item = vm.Items[0];

            vm.DeleteCommand.Execute(item);
            await Task.Delay(50);

            Assert.Empty(vm.Items);
            Assert.Empty(await service.GetAllAsync());
        }
    }
}
