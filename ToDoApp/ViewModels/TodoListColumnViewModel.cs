using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ToDoApp.Models;
using ToDoApp.Services;

namespace ToDoApp.ViewModels
{
    /// <summary>
    /// Representa una columna del board: envuelve un TodoList y sus tareas,
    /// separadas en activas (Items) y completadas (CompletedItems, colapsable).
    /// </summary>
    public class TodoListColumnViewModel : INotifyPropertyChanged
    {
        private readonly TodoService _todoService;
        private readonly TodoListService _todoListService;

        public TodoList List { get; }

        public TodoListColumnViewModel(TodoList list, TodoService todoService, TodoListService todoListService)
        {
            List = list;
            _todoService = todoService;
            _todoListService = todoListService;

            AddTaskCommand = new RelayCommand(AddTask, CanAddTask);
            DeleteTaskCommand = new RelayCommand<TodoItem>(DeleteTask);
        }

        public int Id => List.Id;

        public bool EsPredeterminada => List.EsPredeterminada;

        public string Nombre
        {
            get => List.Nombre;
            set
            {
                if (List.Nombre == value) return;
                if (string.IsNullOrWhiteSpace(value))
                {
                    // no permitir vaciar el nombre; forzar que la UI vuelva a mostrar el actual
                    OnPropertyChanged();
                    return;
                }

                List.Nombre = value;
                OnPropertyChanged();
                _ = _todoListService.RenameAsync(List.Id, value);
            }
        }

        public ObservableCollection<TodoItem> Items { get; } = new ObservableCollection<TodoItem>();
        public ObservableCollection<TodoItem> CompletedItems { get; } = new ObservableCollection<TodoItem>();

        public int CompletedCount => CompletedItems.Count;
        public string CompletedHeaderText => $"Completed ({CompletedCount})";

        private bool _isCompletedExpanded;
        public bool IsCompletedExpanded
        {
            get => _isCompletedExpanded;
            set
            {
                if (_isCompletedExpanded != value)
                {
                    _isCompletedExpanded = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _newTaskTitle = string.Empty;
        public string NewTaskTitle
        {
            get => _newTaskTitle;
            set
            {
                if (_newTaskTitle != value)
                {
                    _newTaskTitle = value;
                    OnPropertyChanged();
                    (AddTaskCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand AddTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }

        /// <summary>Agrega una tarea ya existente (carga inicial, o movida desde otra lista) al bucket correcto.</summary>
        public void AddExistingItem(TodoItem item)
        {
            if (item.Estado == TodoEstado.Completado) CompletedItems.Add(item);
            else Items.Add(item);
            SubscribeItem(item);
            RaiseCompletedCountChanged();
        }

        /// <summary>Deja de rastrear una tarea (se movió a otra lista o se borró), sin tocar la base de datos.</summary>
        public void DetachItem(TodoItem item)
        {
            item.PropertyChanged -= Item_PropertyChanged;
            Items.Remove(item);
            CompletedItems.Remove(item);
            RaiseCompletedCountChanged();
        }

        private bool CanAddTask() => !string.IsNullOrWhiteSpace(NewTaskTitle);

        public async void AddTask()
        {
            if (string.IsNullOrWhiteSpace(NewTaskTitle)) return;

            // Alta rápida: solo título. Descripción y fecha límite se completan
            // después editando la tarjeta, igual que en Sprint 1.
            var item = new TodoItem { Title = NewTaskTitle.Trim(), TodoListId = List.Id };
            await _todoService.AddAsync(item);
            Items.Add(item);
            SubscribeItem(item);
            NewTaskTitle = string.Empty;
            (AddTaskCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        public async void DeleteTask(TodoItem? item)
        {
            if (item is null) return;
            try
            {
                await _todoService.DeleteAsync(item.Id);
            }
            catch
            {
                // ignore for now, igual que en Sprint 1
            }
            DetachItem(item);
        }

        private void SubscribeItem(TodoItem item) => item.PropertyChanged += Item_PropertyChanged;

        private async void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not TodoItem item) return;

            if (e.PropertyName == nameof(TodoItem.Estado))
            {
                MoveToCorrectBucket(item);
            }

            await _todoService.UpdateAsync(item);
        }

        private void MoveToCorrectBucket(TodoItem item)
        {
            var isCompleted = item.Estado == TodoEstado.Completado;
            if (isCompleted && Items.Remove(item))
            {
                CompletedItems.Add(item);
                RaiseCompletedCountChanged();
            }
            else if (!isCompleted && CompletedItems.Remove(item))
            {
                Items.Add(item);
                RaiseCompletedCountChanged();
            }
        }

        private void RaiseCompletedCountChanged()
        {
            OnPropertyChanged(nameof(CompletedCount));
            OnPropertyChanged(nameof(CompletedHeaderText));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
