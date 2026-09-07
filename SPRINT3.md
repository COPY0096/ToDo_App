Sprint 3 - Jerarquía (Subtareas)
================================

Objetivo: permitir que una tarea tenga subtareas (un nivel de anidamiento),
mostradas dentro de la misma tarjeta en el board, con persistencia en SQLite
vía EF Core.

Relación de datos: auto-referencia en `TodoItem` — **una tarea puede tener
muchas subtareas; una subtarea pertenece a una sola tarea padre** (uno a
muchos, `TodoItem` 1 → * `TodoItem`, vía `ParentTaskId`).

Alcance de la jerarquía: **un solo nivel**. Una subtarea no puede a su vez
tener subtareas (`ParentTaskId` de una subtarea siempre `null`-bloqueado a
nivel de servicio). Nesting multi-nivel queda fuera de este sprint — ver
"Fuera de alcance".

---

## Entregables

### 1. Cambios en `TodoItem`

Agregar a `Models/TodoItem.cs`:

```csharp
/// <summary>FK a la tarea padre. Null si es una tarea de primer nivel.</summary>
public int? ParentTaskId { get; set; }

public TodoItem? ParentTask { get; set; }

/// <summary>
/// Subtareas de esta tarea. ObservableCollection (no List) para poder
/// bindear directamente desde XAML sin un ViewModel por tarjeta, siguiendo
/// el mismo patrón "modelo con INotifyPropertyChanged" que ya usa el
/// proyecto en vez de introducir wrappers nuevos.
/// </summary>
public ObservableCollection<TodoItem> SubTasks { get; set; } = new ObservableCollection<TodoItem>();
```

Una subtarea sigue siendo un `TodoItem` normal (mismos campos: título,
descripción, estado, fecha de vencimiento) y sigue perteneciendo a un
`TodoListId` — ver decisión 1 más abajo.

### 2. `AppDbContext`

- Relación auto-referenciada:

```csharp
modelBuilder.Entity<TodoItem>()
    .HasOne(t => t.ParentTask)
    .WithMany(t => t.SubTasks)
    .HasForeignKey(t => t.ParentTaskId)
    .OnDelete(DeleteBehavior.Cascade); // borrar la tarea padre borra sus subtareas
```

- Índice sobre `ParentTaskId` (las columnas ya cargan `WHERE TodoListId = X`;
  ahora además hay que poder pedir `WHERE ParentTaskId = Y` sin table scan).

### 3. Migración EF Core

`dotnet ef migrations add AddSubtasks -o Data/Migrations`

Solo agrega la columna nullable `ParentTaskId` a `TodoItems` + la FK +
el índice. No requiere backfill (las tareas existentes quedan con
`ParentTaskId = null`, es decir, todas siguen siendo tareas de primer
nivel).

### 4. `TodoService`

```csharp
GetSubTasksAsync(int parentId)                         // subtareas de una tarea
AddSubTaskAsync(TodoItem item, int parentTaskId)        // valida profundidad (ver decisión 2)
```

`GetAllAsync()` sigue devolviendo todo (`TodoItems`), pero el ViewModel
filtra: las tareas de primer nivel son las que tienen `ParentTaskId == null`;
el resto se cuelgan de `SubTasks` de su padre al armar el árbol en memoria
(mismo patrón que usa hoy `MainViewModel.InitializeAsync()` para repartir
tareas en columnas por `TodoListId`).

`AddSubTaskAsync` rechaza (lanza `InvalidOperationException`, mismo estilo
que `TodoListService.DeleteAsync`) si `parentTaskId` ya es en sí una
subtarea — así se garantiza el límite de un solo nivel sin depender de una
constraint de base de datos.

### 5. ViewModel

- `TodoListColumnViewModel.AddExistingItem` deja de agregar automáticamente
  *todas* las tareas de la lista a `Items`/`CompletedItems`: primero separa
  tareas de primer nivel (`ParentTaskId == null`) de subtareas, arma
  `SubTasks` de cada padre, y solo las de primer nivel entran a
  `Items`/`CompletedItems` (las subtareas viven colgadas de su padre, no
  como entradas propias de la columna).
- Cada `TodoItem` de primer nivel necesita su propio comando de alta rápida
  de subtarea. Como el proyecto no tiene un ViewModel por tarjeta (las
  tarjetas bindean directo al modelo `TodoItem`), la opción más simple y
  consistente es que `TodoListColumnViewModel` exponga:

```csharp
ICommand AddSubTaskCommand   // parámetro: (TodoItem padre, string título)
ICommand DeleteSubTaskCommand // parámetro: TodoItem subtarea
```

  y sea la columna (no la tarjeta) quien sabe cómo persistir vía
  `_todoService.AddSubTaskAsync(...)` / `DeleteAsync(...)`, igual que ya
  hace hoy con `AddTaskCommand`/`DeleteTaskCommand` para tareas de primer
  nivel.
- Al cambiar el `Estado` de una subtarea se persiste igual que hoy
  (`Item_PropertyChanged` ya escucha `PropertyChanged` de cualquier
  `TodoItem` suscripto); solo hay que suscribir también las subtareas al
  cargarlas.

### 6. UI (XAML)

- Cada tarjeta de tarea de primer nivel agrega, debajo de
  título/descripción/estado/fecha:
  - Lista indentada de sus `SubTasks` (checkbox + título, template simple:
    sin descripción ni fecha propia — ver decisión 3).
  - Fila "+ Add a subtask" (mismo patrón visual que "+ Add a task" de la
    columna), visible siempre que la tarea no sea ella misma una subtarea.
  - Contador simple tipo "2/5" si tiene subtareas (no es un requisito duro,
    pero es casi gratis con `SubTasks.Count(s => s.Estado == Completado)`).
- Las subtareas **no** aparecen como tarjetas sueltas en la columna ni en la
  sección "Completed (N)" de la columna — viven únicamente anidadas dentro
  de la tarjeta de su padre, incluso si están completadas.

---

## Fuera de alcance de este sprint (backlog futuro)

- **Nesting multi-nivel** (subtarea de subtarea) → si hace falta más
  adelante, es un cambio de "límite de profundidad" en el servicio, no de
  modelo (la FK auto-referenciada ya lo soportaría).
- **Mover una subtarea a otra tarea padre**, o **"ascenderla"** a tarea de
  primer nivel → backlog; hoy una subtarea solo se crea y se borra.
- **Reordenar subtareas** (drag & drop o campo `Orden`) → quedan en el
  orden en que se cargan de la base.
- **Rollup automático de estado**: completar todas las subtareas no marca
  la tarea padre como completada automáticamente, y viceversa (marcar el
  padre no completa las subtareas). Es un comportamiento a evaluar en un
  sprint de UX, no algo obvio que todos los usuarios esperarían igual.
- **Mover una tarea con subtareas a otra lista** vía el menú de
  lista/columna → hoy no existe esa función ni para tareas simples
  (Sprint 2 tampoco la implementó), así que tareas-con-subtareas no la
  necesitan resolver primero acá.

---

## Decisiones confirmadas

1. **La subtarea comparte lista con su padre** — `AddSubTaskAsync` fija
   `item.TodoListId = padre.TodoListId` y lo ignora si viene distinto; no
   se expone UI para poner una subtarea en otra columna.
2. **Límite de profundidad = 1 nivel** — una subtarea no puede tener
   subtareas propias.
3. **Plantilla de subtarea simplificada**: solo título + checkbox, sin
   descripción ni fecha de vencimiento propia.
4. **Borrado en cascada silencioso**: eliminar una tarea padre elimina sus
   subtareas directamente, sin diálogo de confirmación extra (a diferencia
   del borrado de listas en Sprint 2, que sí pregunta mover-vs-eliminar).

---

## Nota de implementación: fixup automático de EF Core

`TodoService.GetAllAsync()`/`AddSubTaskAsync()` nunca pueblan
`TodoItem.SubTasks`/`ParentTask` a mano — no hace falta. Como toda la app
usa un único `AppDbContext` de larga duración (`TodoService`/`TodoListService`
están registrados `Scoped`, pero se resuelven una sola vez junto al
`MainViewModel` `Singleton` en `App.xaml.cs`, así que en la práctica es una
sola instancia de contexto durante toda la vida de la app), EF Core hace
**fixup automático de navegación** entre entidades trackeadas del mismo
contexto: en cuanto una tarea y su subtarea están ambas trackeadas y
`ParentTaskId` coincide, EF completa `subtarea.ParentTask` y
`padre.SubTasks` solo, sin que el código de la app tenga que tocarlos.

Si en algún momento se agrega alguna consulta con `AsNoTracking()` en este
camino, o el contexto deja de ser de larga duración, este fixup deja de
pasar y sí haría falta poblar `SubTasks` a mano (como se intentó al
principio de este sprint — terminaba duplicando entradas, porque EF ya lo
hacía solo).

---

## Verificación manual

1. Correr la app sobre una base existente de Sprint 2 → todas las tareas
   existentes siguen viéndose igual (de primer nivel, sin subtareas).
2. En una tarea existente, usar "+ Add a subtask" para crear una subtarea.
3. Cerrar y reabrir la app → la subtarea persiste anidada bajo su tarea
   padre, en la columna correcta.
4. Marcar una subtarea como completada → su checkbox/estado cambia y
   persiste; la tarea padre **no** cambia de estado automáticamente.
5. Intentar agregar una subtarea a una subtarea (si se expone algún punto
   de entrada) → debe estar bloqueado/no disponible en la UI.
6. Borrar la tarea padre → sus subtareas desaparecen también (cascada), sin
   quedar huérfanas en la base.
7. Eliminar la columna donde vive una tarea con subtareas (flujo de
   Sprint 2: mover vs. eliminar) → confirmar que la tarea y todas sus
   subtareas se mueven o se eliminan juntas, nunca separadas.
