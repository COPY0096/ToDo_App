using System.Windows;
using ToDoApp.ViewModels;

namespace ToDoApp.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();
        }

        // DI constructor
        public MainWindow(MainViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.AddItem();
        }
    }
}
