Sprint 2 - Organización (Listas / Board)
========================================

Objetivo: organizar las tareas en listas, mostradas como columnas tipo board
(inspirado en TaskBoard/Google Tasks), con persistencia en SQLite vía EF Core.

Relación de datos: **una lista tiene muchas tareas; una tarea pertenece a una
sola lista** (uno a muchos, `TodoList` 1 → * `TodoItem`).

Lista por defecto: al migrar, se crea automáticamente una lista **"Mis
Tareas"** y todas las tareas existentes (Sprint 1) se le asignan. Esa lista
no se puede eliminar.

---

## Entregables

### 1. Modelo `TodoList`

Nuevo archivo `Models/TodoList.cs`:

```csharp
public class TodoList : INotifyPropertyChanged
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Orden { get; set; }              // posición de la columna en el board
    public DateTime FechaCreacion { get; } = DateTime.Now;
    public bool EsPredeterminada { get; set; }   // true solo para "Mis Tareas"; bloquea el borrado

    public ICollection<TodoItem> Items { get; set; } = new List<TodoItem>();
}
```

### 2. Cambios en `TodoItem`

- Agregar `TodoListId` (int, FK) y navegación `TodoList? TodoList`.
- Toda tarea nueva requiere una lista (`TodoListId` no nulable).

### 3. `AppDbContext`

- `DbSet<TodoList> TodoLists`.
- Relación: `TodoItem.TodoListId` → `TodoList.Id`, `WithMany(l => l.Items)`, `DeleteBehavior.Restrict` (el borrado de tareas al eliminar una lista lo maneja el servicio explícitamente, no una cascada automática de EF — ver punto 5).

### 4. Migración EF Core

`dotnet ef migrations add AddTodoLists -o Data/Migrations`

La migración debe, en el mismo `Up()`:
1. Crear la tabla `TodoLists`.
2. Agregar la columna `TodoListId` a `TodoItems`.
3. Insertar la fila de la lista predeterminada ("Mis Tareas", `EsPredeterminada = true`).
4. Actualizar todas las filas existentes de `TodoItems` para apuntar a esa lista (backfill), antes de poner la FK como `NOT NULL`.

### 5. Servicio: `TodoListService` (nuevo)

```csharp
GetAllAsync()                                        // listas ordenadas por Orden
AddAsync(string nombre)                               // crea lista al final del board
RenameAsync(int id, string nombre)
DeleteAsync(int id, bool moverTareasAMisTareas)       // rechaza si EsPredeterminada
```

**Borrado de lista — confirmado:** cada vez que se elimina una lista no
predeterminada, la UI pregunta explícitamente qué hacer con sus tareas:
**"Mover a Mis Tareas"** o **"Eliminar tareas"** (no hay un comportamiento
fijo global). `DeleteAsync` recibe esa elección:
- `moverTareasAMisTareas = true` → reasigna `TodoListId` de esas tareas a la lista predeterminada, después borra la lista.
- `moverTareasAMisTareas = false` → borra las tareas de esa lista y después la lista.

En el ViewModel esto se traduce en un pequeño diálogo/`MessageBox` con esas
dos opciones (+ Cancelar) antes de invocar `DeleteAsync`.

`TodoService` se ajusta para que `AddAsync(TodoItem)` requiera `TodoListId`,
y `GetAllAsync()` incluya el `Include(TodoList)` o se filtre por lista.

### 6. ViewModel

- `MainViewModel` pasa a exponer `ObservableCollection<TodoListColumnViewModel> Lists` en vez de un único `Items` plano.
- Nuevo `TodoListColumnViewModel`: envuelve un `TodoList`, expone:
  - `Items` (tareas pendientes de esa lista)
  - `CompletedItems` / `CompletedCount` (colapsable, como "Completed (31)" en la referencia)
  - `NewTaskTitle` + `AddTaskCommand` propios (alta rápida "+ Add a task" por columna, sin descripción/fecha — eso se completa después editando la tarjeta, igual que hoy)
  - `RenameCommand`, `DeleteCommand`
- `MainViewModel` agrega `AddListCommand` (para la columna "+ Add new list") y `DeleteListCommand`.

### 7. UI (XAML)

- Reemplazar el `ListBox` único por un `ItemsControl` horizontal (`WrapPanel`/`StackPanel` con `ScrollViewer` horizontal) de columnas.
- Cada columna: header con nombre de la lista + botón "⋮" (por ahora solo **Renombrar** y **Eliminar lista**; el resto del menú de la referencia — Sort By, Share, Set color, Print, Export, Duplicate, Show deleted — queda fuera de este sprint, ver abajo).
- Debajo del header: "+ Add a task" (entrada rápida) y la lista de tarjetas (reutilizando el `DataTemplate` de tarjeta que ya existe: título/descripción editables, combo de Estado con color, fecha límite).
- Al final: sección colapsable "Completed (N)" con las tareas de esa lista en estado Completado.
- Última columna fija: "+ Add new list".
- Tarjeta de tarea: se mantiene el diseño actual (título/descripción editables + combo Estado con color + fecha límite inline). El menú "⋮" por tarea y los íconos de calendario/etiqueta/adjunto que se ven en la referencia (imagen de "Title" / "Details") **no se agregan en este sprint** — ver "Fuera de alcance".

---

## Fuera de alcance de este sprint (backlog futuro)

Tomando la referencia de TaskBoard, esto queda explícitamente **fuera**:

- **Subtareas / tareas anidadas** (ej. "Sprint 1" bajo "App - Lista de QueHaceres") → ya está planificado como **Sprint 3** (jerarquía).
- **Badges de fecha en la tarjeta** tipo "Today" / "3 days ago" → posible mejora chica a futuro sobre `FechaVencimiento`, no se mezcla acá.
- **Integración con Google Tasks API** (sync de tareas externas) → es una integración grande aparte (OAuth, mapeo de datos, sync bidireccional); se trataría como su propio epic, no como parte de organización en listas.
- **Menú completo de lista**: Sort By, Share list, Set color, Print list, Export to Sheet, Duplicate list, Show/restore deleted tasks → backlog. Este sprint solo cubre Renombrar y Eliminar.
- **Papelera de tareas/listas borradas** → backlog (hoy el borrado es definitivo, igual que en Sprint 1).
- **Reordenar columnas por drag & drop** → el campo `Orden` queda listo en el modelo, pero la interacción de arrastrar se puede sumar después; por ahora alcanza con que las listas nuevas se agreguen al final.
- **Etiquetas (tags) y adjuntos por tarea**, y el menú "⋮" por tarea individual (visto en la segunda referencia: ícono de calendario/etiqueta/clip bajo "Details") → son campos nuevos que no existen en `TodoItem` hoy; quedan para un sprint futuro de "metadata de tarea" en vez de mezclarse con la organización en listas.

---

## Verificación manual

1. Correr la app sobre una base existente del Sprint 1 → debe aparecer una única columna "Mis Tareas" con todas las tareas ya creadas.
2. Crear una lista nueva (ej. "Programación") con "+ Add new list".
3. Agregar una tarea directamente en esa columna con "+ Add a task".
4. Cerrar y reabrir la app → la tarea persiste en la columna correcta.
5. Marcar la tarea como Completado → desaparece de la lista activa y aparece bajo "Completed (N)" de esa misma columna.
6. Intentar eliminar "Mis Tareas" → debe estar bloqueado/deshabilitado.
7. Eliminar la columna "Programación" → pregunta "Mover a Mis Tareas" / "Eliminar tareas" / "Cancelar", y procede según lo elegido.
8. Repetir el punto 7 eligiendo la otra opción la próxima vez → confirmar que ambos caminos (mover vs. eliminar) funcionan, ya que no hay un comportamiento fijo.

---

## Decisiones confirmadas

1. **Borrado de lista:** se pregunta en cada caso si mover las tareas a "Mis Tareas" o eliminarlas junto con la lista (no es un comportamiento fijo global) — ver sección 5.
2. **Alta rápida por columna:** "+ Add a task" crea la tarea solo con el título; descripción y fecha límite se completan después editando la tarjeta, igual que en Sprint 1.
