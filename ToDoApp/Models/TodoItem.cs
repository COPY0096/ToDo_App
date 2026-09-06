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

        // Nota: antes era "{ get; }" (sin setter), lo que hacía que EF Core nunca
        // la mapeara a columna (no aparecía en el snapshot del modelo) y que se
        // recalculara a "ahora" cada vez que la entidad se recargaba desde la DB.
        // Con setter, EF la persiste como cualquier otra propiedad.
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

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

        /// <summary>FK a la lista (columna del board) a la que pertenece esta tarea. Requerida.</summary>
        public int TodoListId { get; set; }

        public TodoList? TodoList { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
