using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using ToDoApp.Data;
using ToDoApp.Services;
using ToDoApp.ViewModels;
using ToDoApp.Views;

namespace ToDoApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost? _host;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContext<AppDbContext>(opts => opts.UseSqlite("Data Source=todo.db"));
                    services.AddScoped<TodoService>();
                    services.AddScoped<TodoListService>();
                    services.AddScoped<BackupService>();
                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<MainWindow>();
                })
                .Build();

            // Sprint 4: red de contención ante una migración que rompa algo — una sola
            // copia rotativa (se sobrescribe en cada arranque), no un backup versionado.
            // El mecanismo real de backup/portabilidad que el usuario controla es el
            // export/import manual a JSON (BackupService, botones en MainWindow).
            const string dbPath = "todo.db";
            const string dbBackupPath = "todo.db.bak";
            if (File.Exists(dbPath))
            {
                File.Copy(dbPath, dbBackupPath, overwrite: true);
            }

            // Apply any pending EF Core migrations (creates the DB on first run,
            // and brings the schema up to date on later runs instead of leaving
            // stale databases missing new columns).
            using (var scope = _host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }

            var main = _host.Services.GetRequiredService<MainWindow>();
            main.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_host is not null) await _host.StopAsync();
            _host?.Dispose();
            base.OnExit(e);
        }
    }

}
