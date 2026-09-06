using System.Windows;
using ToDoApp.ViewModels;

namespace ToDoApp.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // DI constructor
        public MainWindow(MainViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
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
    }
}
