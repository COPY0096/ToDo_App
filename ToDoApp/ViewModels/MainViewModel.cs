using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using ToDoApp.Models;
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
                var listItems = items.Where(i => i.TodoListId == list.Id).ToList();

                // Solo las tareas de primer nivel entran a la columna. Sus subtareas
                // (Sprint 3, un solo nivel) NO se agregan a mano: como todas las tareas
                // se cargaron juntas en la misma consulta de un único AppDbContext de
                // larga duración, EF Core ya hizo el fixup de ParentTask/SubTasks entre
                // las entidades trackeadas — item.SubTasks viene poblado solo.
                foreach (var item in listItems.Where(i => i.ParentTaskId is null))
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
        /// Mueve una tarea de primer nivel (y sus subtareas) de su columna actual a
        /// <paramref name="targetColumn"/>. No-op si ya está ahí, o si <paramref name="task"/>
        /// es una subtarea (no se pueden mover de forma independiente de su padre).
        /// </summary>
        public async Task MoveTaskAsync(TodoItem task, TodoListColumnViewModel targetColumn)
        {
            if (task.ParentTaskId is not null) return;
            if (task.TodoListId == targetColumn.Id) return;

            var sourceColumn = Lists.FirstOrDefault(l => l.Id == task.TodoListId);
            sourceColumn?.DetachItem(task);

            await _todoService.MoveToListAsync(task.Id, targetColumn.Id);

            targetColumn.AddExistingItem(task);
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
