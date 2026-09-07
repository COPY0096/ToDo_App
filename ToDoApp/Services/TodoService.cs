using System;
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

        /// <summary>Subtareas de primer nivel de una tarea (Sprint 3).</summary>
        public async Task<IEnumerable<TodoItem>> GetSubTasksAsync(int parentId) =>
            await _db.TodoItems.Where(t => t.ParentTaskId == parentId).ToListAsync();

        /// <summary>
        /// Crea una subtarea bajo <paramref name="parentTaskId"/>. La subtarea hereda la
        /// lista (TodoListId) de su padre. Límite de un solo nivel: rechaza si la tarea
        /// padre es en sí misma una subtarea.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Si la tarea padre no existe o ya es una subtarea (profundidad máxima 1 nivel).
        /// </exception>
        public async Task<TodoItem> AddSubTaskAsync(TodoItem item, int parentTaskId)
        {
            var parent = await _db.TodoItems.FindAsync(parentTaskId);
            if (parent is null)
                throw new InvalidOperationException("La tarea padre no existe.");
            if (parent.ParentTaskId is not null)
                throw new InvalidOperationException("Una subtarea no puede tener subtareas propias.");

            item.ParentTaskId = parentTaskId;
            item.TodoListId = parent.TodoListId;

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
