using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;
using System.Windows.Input;
using System.Threading.Tasks;
using ToDoApp.Models;
using ToDoApp.Services;

namespace ToDoApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly TodoService _service;

        public MainViewModel(TodoService service)
        {
            _service = service;

            // initialize commands
            AddTaskCommand = new RelayCommand(AddItem, CanAdd);
            DeleteCommand = new RelayCommand<TodoItem>(DeleteItem);
        }

        public async Task InitializeAsync()
        {
            var items = await _service.GetAllAsync();
            foreach (var it in items)
            {
                Items.Add(it);
                SubscribeItem(it);
            }
        }

        public ObservableCollection<TodoItem> Items { get; } = new ObservableCollection<TodoItem>();

        private string _newTitle = string.Empty;
        private string _newDescription = string.Empty;
        public string NewTitle
        {
            get => _newTitle;
            set
            {
                if (_newTitle != value)
                {
                    _newTitle = value;
                    OnPropertyChanged();
                    (AddTaskCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string NewDescription
        {
            get => _newDescription;
            set
            {
                if (_newDescription != value)
                {
                    _newDescription = value;
                    OnPropertyChanged();
                }
            }
        }

        public async void AddItem()
        {
            if (string.IsNullOrWhiteSpace(NewTitle)) return;
            var item = new TodoItem { Title = NewTitle, Description = NewDescription };
            await _service.AddAsync(item);
            Items.Add(item);
            SubscribeItem(item);
            NewTitle = string.Empty;
            NewDescription = string.Empty;
            // notify command can execute changed
            (AddTaskCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private void SubscribeItem(TodoItem item)
        {
            item.PropertyChanged += Item_PropertyChanged;
        }

        private async void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is TodoItem item)
            {
                // Persist changes (e.g., IsDone toggles)
                await _service.UpdateAsync(item);
            }
        }

        public async void DeleteItem(TodoItem? item)
        {
            if (item is null) return;
            try
            {
                await _service.DeleteAsync(item.Id);
            }
            catch
            {
                // ignore for now
            }
            item.PropertyChanged -= Item_PropertyChanged;
            Items.Remove(item);
        }

        private bool CanAdd() => !string.IsNullOrWhiteSpace(NewTitle);

        public ICommand AddTaskCommand { get; }
        public ICommand DeleteCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
