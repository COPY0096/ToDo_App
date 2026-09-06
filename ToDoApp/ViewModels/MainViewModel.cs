using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using ToDoApp.Services;

namespace ToDoApp.ViewModels
{
    /// <summary>
    /// Expone el board como una colección de columnas (una por TodoList). Cada
    /// columna administra sus propias tareas; este ViewModel solo orquesta el
    /// alta/baja de listas.
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly TodoService _todoService;
        private readonly TodoListService _todoListService;

        public MainViewModel(TodoService todoService, TodoListService todoListService)
        {
            _todoService = todoService;
            _todoListService = todoListService;

            AddListCommand = new RelayCommand(AddList, CanAddList);
        }

        public ObservableCollection<TodoListColumnViewModel> Lists { get; } = new ObservableCollection<TodoListColumnViewModel>();

        public async Task InitializeAsync()
        {
            var lists = (await _todoListService.GetAllAsync()).ToList();
            var items = (await _todoService.GetAllAsync()).ToList();

            foreach (var list in lists)
            {
                var column = new TodoListColumnViewModel(list, _todoService, _todoListService);
                foreach (var item in items.Where(i => i.TodoListId == list.Id))
                {
                    column.AddExistingItem(item);
                }
                Lists.Add(column);
            }
        }

        private string _newListName = string.Empty;
        public string NewListName
        {
            get => _newListName;
            set
            {
                if (_newListName != value)
                {
                    _newListName = value;
                    OnPropertyChanged();
                    (AddListCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand AddListCommand { get; }

        private bool CanAddList() => !string.IsNullOrWhiteSpace(NewListName);

        public async void AddList()
        {
            if (string.IsNullOrWhiteSpace(NewListName)) return;

            var list = await _todoListService.AddAsync(NewListName.Trim());
            Lists.Add(new TodoListColumnViewModel(list, _todoService, _todoListService));
            NewListName = string.Empty;
            (AddListCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Elimina una lista no predeterminada. Si <paramref name="moverTareasAMisTareas"/> es
        /// true, sus tareas pasan a la lista predeterminada; si es false, se eliminan junto
        /// con la lista. La UI (MainWindow) es responsable de preguntarle al usuario cuál de
        /// las dos opciones quiere antes de llamar a este método.
        /// </summary>
        public async Task DeleteListAsync(TodoListColumnViewModel column, bool moverTareasAMisTareas)
        {
            if (column.EsPredeterminada) return;

            var affectedItems = column.Items.Concat(column.CompletedItems).ToList();

            await _todoListService.DeleteAsync(column.Id, moverTareasAMisTareas);

            foreach (var item in affectedItems)
                column.DetachItem(item);

            Lists.Remove(column);

            if (moverTareasAMisTareas)
            {
                var defaultColumn = Lists.FirstOrDefault(l => l.EsPredeterminada);
                if (defaultColumn is not null)
                {
                    foreach (var item in affectedItems)
                        defaultColumn.AddExistingItem(item);
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
