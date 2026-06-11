using System;
using System.ComponentModel;

namespace ToDoApp.Models
{
    public enum TodoEstado
    {
        Pendiente,
        Completado,
        Cancelado
    }

    public class TodoItem : INotifyPropertyChanged
    {
        public int Id { get; set; }

        private string _title = string.Empty;
        private string _description = string.Empty;
        private bool _isDone;
        private TodoEstado _estado = TodoEstado.Pendiente;
        private DateTime? _fechaVencimiento;

        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        public bool IsDone
        {
            get => _isDone;
            set
            {
                if (_isDone != value)
                {
                    _isDone = value;
                    OnPropertyChanged(nameof(IsDone));
                    // keep Estado in sync
                    Estado = _isDone ? TodoEstado.Completado : TodoEstado.Pendiente;
                }
            }
        }

        public TodoEstado Estado
        {
            get => _estado;
            set
            {
                if (_estado != value)
                {
                    _estado = value;
                    OnPropertyChanged(nameof(Estado));
                    // keep IsDone in sync
                    var done = _estado == TodoEstado.Completado;
                    if (_isDone != done)
                    {
                        _isDone = done;
                        OnPropertyChanged(nameof(IsDone));
                    }
                }
            }
        }

        public DateTime FechaCreacion { get; } = DateTime.Now;

        public DateTime? FechaVencimiento
        {
            get => _fechaVencimiento;
            set
            {
                if (_fechaVencimiento != value)
                {
                    _fechaVencimiento = value;
                    OnPropertyChanged(nameof(FechaVencimiento));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
