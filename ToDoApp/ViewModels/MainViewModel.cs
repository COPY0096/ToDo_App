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
            foreach (var it in _service.GetAll()) Items.Add(it);
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
            NewTitle = string.Empty;
            // notify command can execute changed
            (AddTaskCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private bool CanAdd() => !string.IsNullOrWhiteSpace(NewTitle);

        public ICommand AddTaskCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
