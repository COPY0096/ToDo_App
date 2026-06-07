using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ToDoApp
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<TodoItem> Items { get; } = new ObservableCollection<TodoItem>();

        private string _newTitle = string.Empty;
        public string NewTitle
        {
            get => _newTitle;
            set { if (_newTitle != value) { _newTitle = value; OnPropertyChanged(); } }
        }

        public void AddItem()
        {
            if (string.IsNullOrWhiteSpace(NewTitle)) return;
            Items.Add(new TodoItem { Title = NewTitle });
            NewTitle = string.Empty;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
