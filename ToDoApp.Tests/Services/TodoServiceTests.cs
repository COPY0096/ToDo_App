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
