using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using ToDoApp.Models;
using ToDoApp.Services;
using ToDoApp.ViewModels;

namespace ToDoApp.Views
{
    public partial class MainWindow : Window
    {
        private BackupService? _backupService;

        public MainWindow()
        {
            InitializeComponent();
        }

        // DI constructor
        public MainWindow(MainViewModel vm, BackupService backupService)
        {
            InitializeComponent();
            DataContext = vm;
            _backupService = backupService;
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                await vm.InitializeAsync();
            }
        }

        // La UI es responsable de preguntar qué hacer con las tareas de la lista
        // antes de borrarla (no hay un comportamiento fijo global, ver SPRINT2.md).
        private async void DeleteList_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement fe || fe.DataContext is not TodoListColumnViewModel column) return;
            if (DataContext is not MainViewModel vm) return;

            var totalTareas = column.Items.Count + column.CompletedItems.Count;

            if (totalTareas == 0)
            {
                var confirmar = MessageBox.Show(
                    $"¿Eliminar la lista \"{column.Nombre}\"?",
                    "Eliminar lista",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (confirmar != MessageBoxResult.Yes) return;

                await vm.DeleteListAsync(column, moverTareasAMisTareas: false);
                return;
            }

            var mensaje = $"La lista \"{column.Nombre}\" tiene {totalTareas} tarea(s).\n\n" +
                          "Sí = moverlas a \"Mis Tareas\" y eliminar la lista\n" +
                          "No = eliminar las tareas junto con la lista\n" +
                          "Cancelar = no eliminar nada";

            var resultado = MessageBox.Show(mensaje, "Eliminar lista", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            if (resultado == MessageBoxResult.Cancel) return;

            await vm.DeleteListAsync(column, moverTareasAMisTareas: resultado == MessageBoxResult.Yes);
        }

        // El combo de "Lista" en cada tarjeta también dispara SelectionChanged al
        // poblarse la primera vez con el valor actual (no solo cuando el usuario elige
        // algo distinto); MainViewModel.MoveTaskAsync ya es no-op si la tarea ya está en
        // esa lista, así que ese primer disparo no hace nada.
        private async void MoveTask_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ComboBox combo) return;
            if (combo.DataContext is not TodoItem task) return;
            if (combo.SelectedItem is not TodoListColumnViewModel targetColumn) return;
            if (DataContext is not MainViewModel vm) return;

            await vm.MoveTaskAsync(task, targetColumn);
        }

        private async void ExportBackup_Click(object sender, RoutedEventArgs e)
        {
            if (_backupService is null) return;

            var dialog = new SaveFileDialog
            {
                FileName = $"todo-backup-{DateTime.Now:yyyyMMdd}.json",
                Filter = "Backup JSON (*.json)|*.json"
            };
            if (dialog.ShowDialog() != true) return;

            try
            {
                var backup = await _backupService.ExportAsync();
                var json = JsonSerializer.Serialize(backup, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(dialog.FileName, json);

                MessageBox.Show($"Backup guardado en:\n{dialog.FileName}", "Exportar backup",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo exportar el backup:\n{ex.Message}", "Exportar backup",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // "Restaurar" (no "Importar"): reemplaza TODOS los datos actuales por los del
        // archivo, no los combina — ver decisión 1 de SPRINT4.md. Por eso la confirmación
        // va ANTES de elegir el archivo: el usuario tiene que aceptar el reemplazo total
        // a ciegas, no después de ya haber elegido qué backup usar.
        private async void RestoreBackup_Click(object sender, RoutedEventArgs e)
        {
            if (_backupService is null) return;
            if (DataContext is not MainViewModel vm) return;

            var confirmar = MessageBox.Show(
                "Esto va a reemplazar TODOS los datos actuales por los del archivo. ¿Continuar?",
                "Restaurar backup",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (confirmar != MessageBoxResult.Yes) return;

            var dialog = new OpenFileDialog { Filter = "Backup JSON (*.json)|*.json|Todos los archivos (*.*)|*.*" };
            if (dialog.ShowDialog() != true) return;

            try
            {
                var json = await File.ReadAllTextAsync(dialog.FileName);
                var backup = JsonSerializer.Deserialize<BackupData>(json)
                    ?? throw new InvalidOperationException("El archivo no tiene el formato esperado.");

                await _backupService.ImportAsync(backup);
                await vm.ReloadAsync();

                MessageBox.Show("Backup restaurado correctamente.", "Restaurar backup",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo restaurar el backup:\n{ex.Message}", "Restaurar backup",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
