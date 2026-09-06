using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ToDoApp.Models
{
    /// <summary>
    /// Una columna del board: agrupa tareas (TodoItem). Una lista tiene muchas
    /// tareas; una tarea pertenece a una sola lista.
    /// </summary>
    public class TodoList : INotifyPropertyChanged
    {
        /// <summary>Nombre de la lista predeterminada, sembrada por la migración y no eliminable.</summary>
        public const string NombrePredeterminada = "Mis Tareas";

        public int Id { get; set; }

        private string _nombre = string.Empty;
        public string Nombre
        {
            get => _nombre;
            set
            {
                if (_nombre != value)
                {
                    _nombre = value;
                    OnPropertyChanged(nameof(Nombre));
                }
            }
        }

        /// <summary>Posición de la columna en el board; las listas nuevas se agregan al final.</summary>
        public int Orden { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        /// <summary>True solo para "Mis Tareas". Bloquea el borrado de la lista.</summary>
        public bool EsPredeterminada { get; set; }

        public List<TodoItem> Items { get; set; } = new List<TodoItem>();

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
