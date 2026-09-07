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

        /// <summary>
        /// Mueve una tarea de primer nivel (y sus subtareas, si tiene) a otra lista.
        /// No-op si ya está en esa lista o si <paramref name="taskId"/> no existe.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Si <paramref name="taskId"/> es una subtarea: no se pueden mover de forma
        /// independiente de su tarea padre (ver SPRINT3.md, fuera de alcance).
        /// </exception>
        public async Task MoveToListAsync(int taskId, int targetListId)
        {
            var task = await _db.TodoItems.Include(t => t.SubTasks).FirstOrDefaultAsync(t => t.Id == taskId);
            if (task is null) return;
            if (task.ParentTaskId is not null)
                throw new InvalidOperationException("No se puede mover una subtarea de forma independiente.");
            if (task.TodoListId == targetListId) return;

            task.TodoListId = targetListId;
            foreach (var subTask in task.SubTasks)
                subTask.TodoListId = targetListId;

            await _db.SaveChangesAsync();
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
