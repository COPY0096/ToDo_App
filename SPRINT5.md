Sprint 5 - Búsqueda y Prioridad
================================

Objetivo: dejar de depender solo del color de `Estado` para decidir qué
atacar primero, y poder encontrar una tarea puntual sin escanear todas las
columnas a mano. Dos features chicas y bastante independientes entre sí,
agrupadas en un sprint porque las dos son "usabilidad a medida que crece
el board" — ver la determinación de sprints para el MVP.

---

## Parte A — Prioridad

### 1. Modelo

Nuevo enum en `Models/TodoItem.cs` (mismo archivo que `TodoEstado`, mismo patrón):

```csharp
public enum TodoPrioridad
{
    Baja,
    Media,
    Alta
}
```

Nueva propiedad en `TodoItem`:

```csharp
private TodoPrioridad _prioridad = TodoPrioridad.Media; // default neutro, igual que Estado arranca en Pendiente

public TodoPrioridad Prioridad
{
    get => _prioridad;
    set
    {
        if (_prioridad != value)
        {
            _prioridad = value;
            OnPropertyChanged(nameof(Prioridad));
        }
    }
}
```

No se sincroniza con nada más (a diferencia de `IsDone`↔`Estado`) — es un
campo independiente.

### 2. Migración

`dotnet ef migrations add AddPrioridad -o Data/Migrations`

Columna `Prioridad INTEGER NOT NULL DEFAULT 1` (1 = Media): EF Core scaffolda
el default automáticamente para una columna no-nullable nueva sobre una
tabla con filas existentes, igual que pasó con `TodoListId` en el Sprint 2.
No hace falta backfill manual — todas las tareas existentes quedan en
Media.

### 3. UI: badge de prioridad en la tarjeta

Mismo patrón visual que el badge de `Estado` (`Border` con color +
`ComboBox` sin borde adentro, alimentado por un `ObjectDataProvider` nuevo
para `TodoPrioridad`), puesto al lado del de `Estado`:

| Prioridad | Color |
|---|---|
| Baja | `#E2E3E5` (gris) |
| Media | `#D1ECF1` (celeste) |
| Alta | `#FFE0B2` (naranja — deliberadamente distinto del rojo de `Estado=Cancelado`, para no confundir "cancelada" con "urgente") |

### 4. Orden automático dentro de una columna

**Las tareas de una columna se muestran siempre ordenadas por prioridad**
(Alta → Media → Baja) y, dentro de la misma prioridad, por fecha de
creación. Se aplica tanto a `Items` como a `CompletedItems` de
`TodoListColumnViewModel`.

- `TodoListColumnViewModel` gana un método privado `SortItems(ObservableCollection<TodoItem>)`
  que reordena in-place con `ObservableCollection.Move(...)` (no
  reconstruye la colección, para no perder de vista qué tarjeta es cuál
  mientras se reordena).
- Se llama después de agregar un item (`AddExistingItem`, `AddTask`,
  cuando `MoveToCorrectBucket` cambia una tarea de bucket) y cada vez que
  `Item_PropertyChanged` detecta que cambió `Prioridad` en una tarea que
  ya está en la columna.
- Las subtareas **no** se reordenan por prioridad — quedan en el orden en
  que se cargan, igual que hoy (ver Decisión 2).

---

## Parte B — Búsqueda

### 1. Alcance del match

Un cuadro de búsqueda único, global, arriba del board (al lado del título
"Main Board" / los botones de backup). Filtra **tareas de primer nivel**
por `Title` **o** `Description`, contains case-insensitive. **No** busca
dentro del contenido de las subtareas ni del nombre de la lista — ver
Decisión 3.

### 2. `MainViewModel`

```csharp
private string _searchText = string.Empty;
public string SearchText
{
    get => _searchText;
    set { if (_searchText != value) { _searchText = value; OnPropertyChanged(); } }
}
```

Sin comando ni debounce: es solo un valor que la UI lee vía binding para
decidir visibilidad, filtra en tiempo real con cada tecla (el board nunca
tiene tantas tareas como para que esto sea un problema de performance).

### 3. Filtro visual: `TaskSearchVisibilityConverter` (nuevo)

Primer `IValueConverter`/`IMultiValueConverter` del proyecto — hasta ahora
toda la UI condicional se resolvía con `DataTrigger` (comparación por
igualdad), pero "contiene la subcadena X" no se puede expresar con un
trigger declarativo.

`Views/Converters/TaskSearchMatcher.cs` — lógica pura y testeable, separada
de la clase que implementa `IMultiValueConverter`:

```csharp
public static class TaskSearchMatcher
{
    public static bool Matches(string title, string description, string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText)) return true;
        return title.Contains(searchText, StringComparison.OrdinalIgnoreCase)
            || description.Contains(searchText, StringComparison.OrdinalIgnoreCase);
    }
}

public class TaskSearchVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var title = values.ElementAtOrDefault(0) as string ?? string.Empty;
        var description = values.ElementAtOrDefault(1) as string ?? string.Empty;
        var searchText = values.ElementAtOrDefault(2) as string;
        return TaskSearchMatcher.Matches(title, description, searchText)
            ? Visibility.Visible : Visibility.Collapsed;
    }

    public object[] ConvertBack(...) => throw new NotSupportedException();
}
```

En la tarjeta (plantilla implícita de `TodoItem` en `MainWindow.xaml`), la
`Visibility` del `Border` raíz pasa a ser un `MultiBinding`:

```xml
<Border.Visibility>
    <MultiBinding Converter="{StaticResource TaskSearchVisibilityConverter}">
        <Binding Path="Title" />
        <Binding Path="Description" />
        <Binding Path="DataContext.SearchText" RelativeSource="{RelativeSource AncestorType=Window}" />
    </MultiBinding>
</Border.Visibility>
```

Aplica igual en la lista `Items` como en `CompletedItems` — buscar
encuentra tareas completadas también (si el usuario tiene esa sección
expandida).

Las columnas en sí **nunca** se ocultan, aunque queden con cero tarjetas
visibles por el filtro — representan listas persistentes, no son "grupos
de resultados" (ver Decisión 4).

---

## Fuera de alcance de este sprint (backlog futuro)

- **Selector de criterio de orden** (por fecha límite, manual, etc.) — el
  orden queda fijo en prioridad→fecha de creación; un selector de "orden
  por X" es una feature aparte si hace falta.
- **Reordenar tareas a mano (drag & drop)** dentro de una columna — no
  tiene sentido mientras el orden es automático por prioridad.
- **Buscar dentro de subtareas** o por nombre de lista — la búsqueda es
  solo título/descripción de tareas de primer nivel.
- **Resaltar la coincidencia** (highlight del texto encontrado) — solo se
  filtra qué se ve, no se decora el texto que matchea.
- **Búsquedas guardadas / filtros combinados** (por prioridad + texto a
  la vez, etc.) — un solo cuadro de texto simple alcanza para este sprint.
- **Prioridad en subtareas** — las subtareas no tienen campo `Prioridad`
  ni participan del orden automático; comparten el resto de las
  limitaciones ya fijadas en Sprint 3 (plantilla simplificada).

---

## Decisiones confirmadas

1. **`Prioridad` default = Media** para tareas nuevas y para las
   existentes al migrar (mismo criterio "valor neutro" que `Estado`
   arrancando en `Pendiente`).
2. **El orden automático por prioridad no aplica a subtareas** — se
   mantienen en su orden de carga, para no ampliar el alcance del Sprint 3
   retroactivamente.
3. **La búsqueda no mira subtareas**, solo título/descripción de la tarea
   de primer nivel — si se necesita más adelante, es una extensión al
   `TaskSearchMatcher`, no un rediseño.
4. **Las columnas nunca se ocultan por la búsqueda**, solo sus tarjetas —
   una lista vacía por el filtro sigue siendo una lista real, visible.

---

## Verificación manual

1. Cambiar la prioridad de una tarea a "Alta" → debe subir al principio
   de su columna (por encima de las "Media"/"Baja" existentes).
2. Crear varias tareas con distinta prioridad → confirmar el orden
   Alta → Media → Baja dentro de la misma columna.
3. Marcar una tarea "Alta" como completada → debe seguir ordenada por
   prioridad dentro de "Completed (N)", no perder el orden al cambiar de
   bucket.
4. Escribir en el buscador un texto que matchea el título de una tarea en
   una columna → solo esa tarjeta (y las que también matcheen) quedan
   visibles; el resto de las tarjetas de esa y otras columnas se ocultan,
   pero las columnas en sí siguen ahí.
5. Buscar un texto que matchea la descripción (no el título) → también
   debe encontrarla.
6. Buscar un texto que matchea el título de una subtarea (no del padre) →
   la tarjeta del padre **no** debe aparecer (confirma que la búsqueda no
   mira subtareas, según Decisión 3).
7. Borrar el texto del buscador → todas las tarjetas vuelven a verse.
