using System;
using Microsoft.EntityFrameworkCore;
using ToDoApp.Models;

namespace ToDoApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TodoItem> TodoItems { get; set; } = null!;
        public DbSet<TodoList> TodoLists { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Una lista tiene muchas tareas; una tarea pertenece a una sola lista.
            // Restrict: el borrado de tareas al eliminar una lista lo decide y ejecuta
            // explícitamente TodoListService (mover vs. eliminar), no una cascada de EF.
            modelBuilder.Entity<TodoItem>()
                .HasOne(t => t.TodoList)
                .WithMany(l => l.Items)
                .HasForeignKey(t => t.TodoListId)
                .OnDelete(DeleteBehavior.Restrict);

            // Lista predeterminada sembrada por la migración (y visible también en
            // los tests que usan el proveedor InMemory, que no corre migraciones).
            modelBuilder.Entity<TodoList>().HasData(new TodoList
            {
                Id = 1,
                Nombre = TodoList.NombrePredeterminada,
                Orden = 0,
                EsPredeterminada = true,
                FechaCreacion = new DateTime(2026, 9, 5, 0, 0, 0, DateTimeKind.Local)
            });
        }
    }
}
