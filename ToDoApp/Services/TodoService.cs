using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ToDoApp.Data;
using ToDoApp.Models;

namespace ToDoApp.Services
{
    public class TodoService
    {
        private readonly AppDbContext _db;
        public TodoService(AppDbContext db) { _db = db; }

        // Async implementations
        public async Task<IEnumerable<TodoItem>> GetAllAsync() => await _db.TodoItems.ToListAsync();

        public async Task<TodoItem> AddAsync(TodoItem item)
        {
            _db.TodoItems.Add(item);
            await _db.SaveChangesAsync();
            return item;
        }

        public async Task<TodoItem> UpdateAsync(TodoItem item)
        {
            _db.TodoItems.Update(item);
            await _db.SaveChangesAsync();
            return item;
        }

        public async Task DeleteAsync(int id)
        {
            var item = await _db.TodoItems.FindAsync(id);
            if (item is null) return;
            _db.TodoItems.Remove(item);
            await _db.SaveChangesAsync();
        }

        // Synchronous wrappers for existing callers
        public IEnumerable<TodoItem> GetAll() => GetAllAsync().GetAwaiter().GetResult();

        public TodoItem Add(TodoItem item) => AddAsync(item).GetAwaiter().GetResult();

        public TodoItem Update(TodoItem item) => UpdateAsync(item).GetAwaiter().GetResult();

        public void Delete(int id) => DeleteAsync(id).GetAwaiter().GetResult();
    }
}
