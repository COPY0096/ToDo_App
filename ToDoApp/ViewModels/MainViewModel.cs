using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;
using System.Windows.Input;
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
            foreach (var it in _service.GetAll())
            {
                Items.Add(it);
                SubscribeItem(it);
            }

            // initialize commands
            AddTaskCommand = new RelayCommand(AddItem, CanAdd);
            DeleteCommand = new RelayCommand<TodoItem>(DeleteItem);
        }

        public ObservableCollection<TodoItem> Items { get; } = new ObservableCollection<TodoItem>();

        private string _newTitle = string.Empty;
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

        public void AddItem()
        {
            if (string.IsNullOrWhiteSpace(NewTitle)) return;
            var item = new TodoItem { Title = NewTitle };
            _service.Add(item);
            Items.Add(item);
            SubscribeItem(item);
            NewTitle = string.Empty;
            // notify command can execute changed
            (AddTaskCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private void SubscribeItem(TodoItem item)
        {
            item.PropertyChanged += Item_PropertyChanged;
        }

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is TodoItem item)
            {
                // Persist changes (e.g., IsDone toggles)
                _service.Update(item);
            }
        }

        public void DeleteItem(TodoItem? item)
        {
            if (item is null) return;
            try
            {
                _service.Delete(item.Id);
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
