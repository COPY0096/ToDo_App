using Microsoft.EntityFrameworkCore;
using ToDoApp.Data;
using ToDoApp.Models;
using ToDoApp.Services;

namespace ToDoApp.Tests.Services
{
    public class TodoServiceTests
    {
        // Each test gets its own isolated in-memory database so tests can run
        // in parallel / any order without sharing state.
        private static TodoService CreateService(out AppDbContext db)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            db = new AppDbContext(options);
            return new TodoService(db);
        }

        [Fact]
        public async Task AddAsync_PersistsItem_AndAssignsId()
        {
            var service = CreateService(out var db);
            var item = new TodoItem { Title = "Comprar pan", Description = "Integral" };

            var added = await service.AddAsync(item);

            Assert.True(added.Id > 0);
            Assert.Equal(1, await db.TodoItems.CountAsync());
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllPersistedItems()
        {
            var service = CreateService(out _);
            await service.AddAsync(new TodoItem { Title = "Tarea 1" });
            await service.AddAsync(new TodoItem { Title = "Tarea 2" });

            var items = (await service.GetAllAsync()).ToList();

            Assert.Equal(2, items.Count);
            Assert.Contains(items, i => i.Title == "Tarea 1");
            Assert.Contains(items, i => i.Title == "Tarea 2");
        }

        [Fact]
        public async Task UpdateAsync_PersistsChanges()
        {
            var service = CreateService(out var db);
            var item = await service.AddAsync(new TodoItem { Title = "Original" });

            item.Title = "Modificado";
            item.IsDone = true;
            await service.UpdateAsync(item);

            var reloaded = await db.TodoItems.FindAsync(item.Id);
            Assert.NotNull(reloaded);
            Assert.Equal("Modificado", reloaded!.Title);
            Assert.True(reloaded.IsDone);
        }

        [Fact]
        public async Task DeleteAsync_RemovesItem()
        {
            var service = CreateService(out var db);
            var item = await service.AddAsync(new TodoItem { Title = "Para borrar" });

            await service.DeleteAsync(item.Id);

            Assert.Equal(0, await db.TodoItems.CountAsync());
        }

        [Fact]
        public async Task DeleteAsync_NonExistentId_DoesNotThrow()
        {
            var service = CreateService(out _);

            var exception = await Record.ExceptionAsync(() => service.DeleteAsync(999));

            Assert.Null(exception);
        }

        [Fact]
        public async Task AddSubTaskAsync_SetsParentId_AndInheritsParentList()
        {
            var service = CreateService(out var db);
            var parent = await service.AddAsync(new TodoItem { Title = "Padre", TodoListId = 7 });
            var subTask = new TodoItem { Title = "Hija", TodoListId = 999 }; // debe ser ignorado

            var added = await service.AddSubTaskAsync(subTask, parent.Id);

            Assert.Equal(parent.Id, added.ParentTaskId);
            Assert.Equal(7, added.TodoListId);
            Assert.Equal(2, await db.TodoItems.CountAsync());
        }

        [Fact]
        public async Task AddSubTaskAsync_ThrowsIfParentDoesNotExist()
        {
            var service = CreateService(out _);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.AddSubTaskAsync(new TodoItem { Title = "Huérfana" }, parentTaskId: 999));
        }

        [Fact]
        public async Task AddSubTaskAsync_ThrowsIfParentIsAlreadyASubTask()
        {
            var service = CreateService(out _);
            var grandParent = await service.AddAsync(new TodoItem { Title = "Abuela" });
            var parent = await service.AddSubTaskAsync(new TodoItem { Title = "Madre" }, grandParent.Id);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.AddSubTaskAsync(new TodoItem { Title = "Nieta" }, parent.Id));
        }

        [Fact]
        public async Task GetSubTasksAsync_ReturnsOnlyDirectChildrenOfThatParent()
        {
            var service = CreateService(out _);
            var parent = await service.AddAsync(new TodoItem { Title = "Padre" });
            var otherParent = await service.AddAsync(new TodoItem { Title = "Otro padre" });
            await service.AddSubTaskAsync(new TodoItem { Title = "Hija 1" }, parent.Id);
            await service.AddSubTaskAsync(new TodoItem { Title = "Hija 2" }, parent.Id);
            await service.AddSubTaskAsync(new TodoItem { Title = "No es hija de parent" }, otherParent.Id);

            var subTasks = (await service.GetSubTasksAsync(parent.Id)).ToList();

            Assert.Equal(2, subTasks.Count);
            Assert.All(subTasks, s => Assert.Equal(parent.Id, s.ParentTaskId));
        }

        [Fact]
        public async Task DeleteAsync_OfParent_CascadesToSubTasks()
        {
            var service = CreateService(out var db);
            var parent = await service.AddAsync(new TodoItem { Title = "Padre" });
            await service.AddSubTaskAsync(new TodoItem { Title = "Hija" }, parent.Id);

            await service.DeleteAsync(parent.Id);

            Assert.Equal(0, await db.TodoItems.CountAsync());
        }

        [Fact]
        public void SynchronousWrappers_DelegateTo_AsyncImplementations()
        {
            var service = CreateService(out var db);

            var added = service.Add(new TodoItem { Title = "Sync add" });
            Assert.True(added.Id > 0);

            var all = service.GetAll().ToList();
            Assert.Single(all);

            added.Title = "Sync updated";
            service.Update(added);
            Assert.Equal("Sync updated", db.TodoItems.Find(added.Id)!.Title);

            service.Delete(added.Id);
            Assert.Empty(service.GetAll());
        }
    }
}
