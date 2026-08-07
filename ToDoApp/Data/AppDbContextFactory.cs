using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ToDoApp.Data
{
    /// <summary>
    /// Allows the `dotnet ef` CLI tools to construct AppDbContext at design time
    /// (e.g. for `dotnet ef migrations add`), since this WPF app builds its
    /// DbContext through a generic Host/DI container rather than a Program.cs
    /// with a static entry point EF can discover on its own.
    /// </summary>
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlite("Data Source=todo.db");
            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
