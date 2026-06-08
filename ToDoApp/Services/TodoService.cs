using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToDoApp.Data;
using ToDoApp.Models;

namespace ToDoApp.Services
{
    public class TodoService
    {
        private readonly AppDbContext _db;
        public TodoService(AppDbContext db) { _db = db; }

        public IEnumerable<TodoItem> GetAll() => _db.TodoItems.AsNoTracking().ToList();

        public TodoItem Add(TodoItem item)
        {
            _db.TodoItems.Add(item);
            _db.SaveChanges();
            return item;
        }
    }
}
