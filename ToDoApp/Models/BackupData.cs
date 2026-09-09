using System;
using System.Collections.Generic;

namespace ToDoApp.Models
{
    /// <summary>
    /// Forma del archivo de backup (Sprint 4): un árbol auto-contenido, sin los
    /// Ids internos de EF Core — no tienen sentido en otra instalación y evitan
    /// referencias circulares (TodoList.Items / TodoItem.ParentTask-SubTasks) al
    /// serializar con System.Text.Json. Al importar, las relaciones se
    /// reconstruyen por posición en el árbol (lista → tareas → subtareas).
    /// </summary>
    public class BackupData
    {
        public DateTime ExportedAt { get; set; }
        public List<TodoListBackup> Lists { get; set; } = new List<TodoListBackup>();
    }

    public class TodoListBackup
    {
        public string Nombre { get; set; } = string.Empty;
        public int Orden { get; set; }
        public bool EsPredeterminada { get; set; }
        public List<TodoItemBackup> Tareas { get; set; } = new List<TodoItemBackup>();
    }

    public class TodoItemBackup
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TodoEstado Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public List<TodoItemBackup> SubTareas { get; set; } = new List<TodoItemBackup>();
    }
}
