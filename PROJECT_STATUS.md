# ToDo App — Estado Actual del Proyecto

**Última actualización:** 8 de septiembre de 2026

---

## 📋 Estado General

El proyecto corresponde a una aplicación de escritorio desarrollada con **WPF**, siguiendo el patrón **MVVM**, utilizando **Entity Framework Core**, **SQLite** y **Microsoft.Extensions.Hosting** para la inyección de dependencias.

**Estado de compilación:** ✅ Compila correctamente sin errores ni warnings

**Rama activa:** `main`

**Último commit:** Sprint 5 (búsqueda y prioridad, post-MVP) — 🟡 implementado, falta verificación manual.

---

## 🛠 Tecnologías Utilizadas

| Tecnología | Versión | Propósito |
|---|---|---|
| **.NET / WPF** | .NET Framework | Frontend desktop |
| **MVVM Pattern** | — | Arquitectura |
| **Entity Framework Core** | v10.25.x | ORM |
| **SQLite** | — | Base de datos local |
| **Microsoft.Extensions.Hosting** | — | Host genérico |
| **Microsoft.Extensions.DependencyInjection** | — | Inyección de dependencias |

---

## 🏗 Arquitectura Actual

```plaintext
ToDoApp/
├── Data/
│   ├── AppDbContext.cs                    ✓ EF Core + relación TodoList 1→* TodoItem + seed
│   ├── AppDbContextFactory.cs             ✓ Design-time factory para `dotnet ef`
│   └── Migrations/                        ✓ InitialCreate, AddTodoLists
│
├── Models/
│   ├── TodoItem.cs                        ✓ Modelo con INotifyPropertyChanged + TodoListId
│   └── TodoList.cs                        ✓ Columna del board (Sprint 2)
│
├── Services/
│   ├── TodoService.cs                     ✓ CRUD con async/await
│   └── TodoListService.cs                 ✓ CRUD de listas + borrado mover/eliminar (Sprint 2)
│
├── ViewModels/
│   ├── MainViewModel.cs                   ✓ Orquesta las columnas del board
│   ├── TodoListColumnViewModel.cs         ✓ Una columna: tareas activas/completadas (Sprint 2)
│   └── RelayCommand.cs                    ✓ Implementado
│
├── Views/
│   ├── MainWindow.xaml                    ✓ Board de columnas con bindings
│   └── MainWindow.xaml.cs                 ✓ Code-behind (init + confirmación de borrado de lista)
│
├── App.xaml                               ✓ Configuración de app
├── App.xaml.cs                            ✓ Inicialización con DI
└── ToDoApp.csproj                         ✓ Proyecto compilando

```

---

## 📊 Estado de los Sprints

### Sprint 0 — Preparación del Proyecto

**Estado:** ✅ **COMPLETADO (100%)**

**Objetivos alcanzados:**
- ✅ Proyecto WPF creado
- ✅ Arquitectura MVVM implementada
- ✅ Organización por capas (Models, Data, Services, ViewModels, Views)
- ✅ Entity Framework Core configurado
- ✅ SQLite configurado
- ✅ Generic Host con Dependency Injection
- ✅ DbContext registrado en contenedor DI
- ✅ Base de datos gestionada con migraciones EF Core (`Database.Migrate()`)
- ✅ Proyecto compilando sin errores

---

### Sprint 1 — Gestión de Tareas (CRUD Básico)

**Estado:** ✅ **COMPLETADO (100%)** — edición en línea, estados visuales, validaciones descriptivas y etiquetas claras agregadas.

#### **Core CRUD**

| Operación | Estado | Detalles |
|---|---|---|
| **Create** | ✅ Completo | `AddAsync()` / `Add()` - Crea y persiste en SQLite |
| **Read** | ✅ Completo | `GetAllAsync()` / `GetAll()` - Carga con tracking |
| **Update** | ✅ Completo | `UpdateAsync()` / `Update()` - Persiste cambios |
| **Delete** | ✅ Completo | `DeleteAsync()` / `Delete()` - Elimina de DB |

---

#### **Modelo TodoItem**

```csharp
public class TodoItem : INotifyPropertyChanged
{
	public int Id { get; set; }
	public string Title { get; set; }
	public string Description { get; set; }
	public bool IsDone { get; set; }
	public TodoEstado Estado { get; set; }  // Enum: Pendiente, Completado, Cancelado
	public DateTime? FechaVencimiento { get; set; }
	public DateTime FechaCreacion { get; set; }

	// Implementa INotifyPropertyChanged para binding automático
}
```

**Campos MVP:**
- ✅ `Id` - Identificador único
- ✅ `Título` - Nombre de la tarea
- ✅ `Descripción` - Detalles
- ✅ `FechaCreacion` - Timestamp de creación
- ✅ `FechaVencimiento` - Fecha límite
- ✅ `Estado` - Enum (Pendiente/Completado/Cancelado)
- ✅ `IsDone` - Flag de completitud

---

#### **Servicios Implementados**

**TodoService** (`Services/TodoService.cs`)

```csharp
// Métodos async
GetAllAsync()                    ✅ Retorna todas las tareas
AddAsync(TodoItem item)         ✅ Crea y guarda tarea
UpdateAsync(TodoItem item)      ✅ Actualiza tarea existente
DeleteAsync(int id)             ✅ Elimina tarea por ID

// Wrappers síncronos para compatibilidad
GetAll()
Add(TodoItem item)
Update(TodoItem item)
Delete(int id)
```

**Características:**
- ✅ Operaciones asíncronas (async/await)
- ✅ Tracking automático de cambios (sin `AsNoTracking()`)
- ✅ Persistencia inmediata con `SaveChangesAsync()`
- ✅ Wrappers síncronos para código legado

---

#### **ViewModel Principal**

**MainViewModel** (`ViewModels/MainViewModel.cs`)

**Propiedades:**
```csharp
Items                          ✅ ObservableCollection<TodoItem>
NewTitle                       ✅ string - Título de nueva tarea
NewDescription                 ✅ string - Descripción de nueva tarea
```

**Comandos:**
```csharp
AddTaskCommand                 ✅ Crea nueva tarea
DeleteCommand<TodoItem>        ✅ Elimina tarea seleccionada
```

**Métodos:**
```csharp
InitializeAsync()              ✅ Carga tareas de DB al iniciar
SubscribeItem(TodoItem)        ✅ Suscribe a cambios de IsDone
```

**Inicio de aplicación:**
- ✅ Se dispara automáticamente al cargar MainWindow
- ✅ Carga todas las tareas existentes en `Items`
- ✅ Suscribe cada item a cambios para persistencia

---

#### **Interfaz de Usuario**

**MainWindow** (`Views/MainWindow.xaml`)

```xaml
Elementos:
├── TextBox para Title               ✅ Binding: NewTitle
├── TextBox para Description         ✅ Binding: NewDescription
├── Button "Add"                     ✅ Command: AddTaskCommand
└── ListBox con DataTemplate         ✅ Muestra Items
	├── CheckBox para IsDone         ✅ Binding bidireccional
	├── TextBlock para Title         ✅ Binding: Title
	├── TextBlock para Description   ✅ Binding: Description
	└── Button "Delete"              ✅ Command: DeleteCommand
```

**Funcionalidad:**
- ✅ Entrada de datos en tiempo real (UpdateSourceTrigger=PropertyChanged)
- ✅ Validación: No permite tareas vacías
- ✅ Checkbox funcional para marcar completadas
- ✅ Delete button por item
- ✅ Event `Grid_Loaded` para inicialización async

---

#### **Persistencia**

| Aspecto | Estado | Detalles |
|---|---|---|
| **Base de datos** | ✅ | SQLite local - Auto-creada |
| **Creación de tareas** | ✅ | Almacenadas automáticamente |
| **Modificación de tareas** | ✅ | IsDone persiste al cambiar |
| **Eliminación de tareas** | ✅ | Removidas de la DB |
| **Carga al iniciar** | ✅ | Se recuperan todas las tareas |
| **Cambios tras reinicio** | ✅ | Se conservan |

---

---

### Sprint 2 — Organización (Listas / Board)

**Estado:** ✅ **COMPLETADO (100%)** — modelo, migración, servicios, ViewModel, UI y verificación manual en la app real, todo hecho (ver [SPRINT2.md](../SPRINT2.md)).

| Ítem | Estado | Detalles |
|---|---|---|
| Modelo `TodoList` | ✅ | `Id`, `Nombre`, `Orden`, `FechaCreacion`, `EsPredeterminada` |
| `TodoItem.TodoListId` (FK) | ✅ | Requerida; relación 1 lista → * tareas, `DeleteBehavior.Restrict` |
| Lista predeterminada "Mis Tareas" | ✅ | Sembrada vía `HasData` (Id=1); no eliminable |
| Migración `AddTodoLists` | ✅ | Backfill automático: tareas existentes del Sprint 1 quedan asignadas a "Mis Tareas" (`DEFAULT 1` en la columna nueva) |
| `TodoListService` | ✅ | CRUD de listas; `DeleteAsync` pregunta mover-vs-eliminar tareas, no es un comportamiento fijo |
| Board en `MainWindow.xaml` | ✅ | Columnas por lista, alta rápida "+ Add a task", sección "Completed (N)" colapsable, "+ Add new list" |
| Tests | ✅ | 37/37 en verde (`TodoListServiceTests` nuevo + `MainViewModelTests` reescrito para columnas) |
| Verificación manual en la app | ✅ | Confirmado por el usuario: "Mis Tareas" sin botón de eliminar, listas nuevas ("Ayuntamiento") sí lo tienen, alta de lista y de tarea funcionan |

**Fuera de alcance de este sprint** (ver [SPRINT2.md](../SPRINT2.md) para el detalle): subtareas (Sprint 3), integración con Google Tasks, menú completo de lista (Sort/Share/Print/Export/papelera), etiquetas/adjuntos por tarea, drag & drop de columnas.

**Bug preexistente corregido de paso:** `TodoItem.FechaCreacion` no tenía setter, por lo que EF Core nunca la mapeaba a columna (no aparecía en el snapshot del modelo) y se recalculaba a "ahora" en cada carga desde la DB. Ahora tiene setter y se persiste correctamente; las tareas creadas antes de este fix quedan con una fecha centinela (`0001-01-01`) ya que su fecha real de creación nunca se guardó.

---

### Sprint 3 — Jerarquía (Subtareas)

**Estado:** ✅ **COMPLETADO (100%)** — modelo, migración, servicios, ViewModel, UI, tests
y verificación manual en la app real, todo hecho siguiendo el plan de [SPRINT3.md](../SPRINT3.md).

| Ítem | Estado | Detalles |
|---|---|---|
| Auto-referencia en `TodoItem` | ✅ | `ParentTaskId` (nullable) + `ParentTask` + `SubTasks` (`ObservableCollection<TodoItem>`), un solo nivel |
| Migración `AddSubtasks` | ✅ | Columna nullable + FK + índice, sin backfill (tareas existentes quedan como tareas de primer nivel) |
| `TodoService` | ✅ | `GetSubTasksAsync`, `AddSubTaskAsync` (hereda la lista del padre, rechaza subtarea-de-subtarea) |
| `TodoListColumnViewModel` | ✅ | `AddSubTaskCommand`/`DeleteSubTaskCommand`; las subtareas nunca cuentan como tarjeta propia de la columna |
| UI en `MainWindow.xaml` | ✅ | Subtareas indentadas dentro de la tarjeta del padre (plantilla simplificada: checkbox + título) + "+ Add a subtask" |
| Tests | ✅ | 45/45 en verde (8 nuevos: `TodoServiceTests` + `MainViewModelTests` para subtareas) |
| Verificación manual en la app | ✅ | Confirmado por el usuario: el flujo completo (agregar subtarea, completarla, borrado en cascada) funciona bien |

**Decisiones tomadas para este sprint** (ver [SPRINT3.md](../SPRINT3.md)): subtarea
comparte lista con su padre, límite de un solo nivel, plantilla simplificada (sin
descripción ni fecha propia), borrado en cascada sin diálogo de confirmación extra.

**Nota de implementación:** no hizo falta poblar `SubTasks` a mano en el ViewModel — EF
Core hace fixup automático de esa navegación entre entidades trackeadas, porque toda la
app comparte un único `AppDbContext` de larga duración. El primer intento poblaba la
colección a mano y terminaba duplicando entradas por esto mismo (detalle en SPRINT3.md).

---

### Mover tareas entre listas

**Estado:** ✅ **Implementado y verificado manualmente en la app real.** No estaba
en el alcance de ningún sprint (Sprint 2 no lo incluyó — ver su "Fuera de alcance"; Sprint
3 lo excluye explícitamente para subtareas) hasta que se pidió como feature aparte.

- Selector "Lista" en cada tarjeta de tarea de primer nivel (`ComboBox` con las columnas
  del board); elegir otra lista mueve la tarea al instante. Las subtareas se mueven
  siempre junto con su tarea padre (mantienen la misma lista); una subtarea no se puede
  mover de forma independiente (`MainViewModel.MoveTaskAsync` es no-op para subtareas).
- `TodoService.MoveToListAsync` persiste el cambio de `TodoListId` de la tarea y de todas
  sus subtareas en una sola operación.
- **Bug preexistente corregido de paso:** `TodoListColumnViewModel.DetachItem` no
  desuscribía las subtareas de una tarea al sacarla de la columna (solo la tarea misma),
  dejándolas escuchando el `PropertyChanged` de la columna vieja. No importaba mientras
  las tareas no se movían entre columnas; con esta feature sí, así que se corrigió.
- Tests: 7 nuevos (`TodoServiceTests.MoveToListAsync_*` + `MainViewModelTests.MoveTaskAsync_*`).
- Verificación manual en la app: ✅ confirmado por el usuario, funciona bien.

---

### Sprint 4 — Confiabilidad de datos (Backup / Export / Import)

**Estado:** ✅ **COMPLETADO (100%)** — modelo, servicio, UI, tests y verificación manual
en la app real, todo hecho siguiendo el plan de [SPRINT4.md](../SPRINT4.md).

| Ítem | Estado | Detalles |
|---|---|---|
| `BackupData`/`TodoListBackup`/`TodoItemBackup` | ✅ | Árbol auto-contenido sin Ids de EF, evita referencias circulares al serializar |
| `BackupService.ExportAsync` | ✅ | Lee explícitamente (no depende del fixup automático de EF); anida subtareas bajo su padre |
| `BackupService.ImportAsync` | ✅ | Reemplazo total en transacción (SQLite real; en tests con InMemory se omite porque ese proveedor no la soporta); si ningún list viene marcado predeterminado, se elige el primero; rechaza backups sin listas; ignora niveles de subtarea más allá del primero |
| UI en `MainWindow.xaml` | ✅ | Botones "Exportar backup" / "Restaurar backup" junto al título; confirmación antes de restaurar, errores descriptivos si el archivo es inválido |
| Backup automático (`todo.db.bak`) | ✅ | Copia rotativa de `todo.db` antes de cada `Database.Migrate()`, red de contención ante una migración rota |
| Tests | ✅ | 60/60 en verde (8 nuevos: `BackupServiceTests` — export, import, reemplazo, jerarquía, límite de nivel, round-trip) |
| Verificación manual en la app | ✅ | Confirmado por el usuario: corrió la app y el flujo de backup/restore funciona bien |

**Decisiones tomadas para este sprint** (ver [SPRINT4.md](../SPRINT4.md)): importar es
reemplazo total (no merge) — por eso el botón dice "Restaurar" y no "Importar"; el `.bak`
automático es solo contención ante una migración rota, no el backup real que controla el
usuario; formato JSON legible sin encriptar.

---

### Sprint 5 — Búsqueda y Prioridad (post-MVP)

**Estado:** 🟡 **Implementado, pendiente verificación manual en la app real** (modelo,
migración, ordenamiento, converter, UI y tests unitarios hechos siguiendo el plan de
[SPRINT5.md](../SPRINT5.md); falta que confirmes el flujo a mano en la app).

| Ítem | Estado | Detalles |
|---|---|---|
| Enum `TodoPrioridad` (Baja/Media/Alta) | ✅ | Default `Media`, independiente de `Estado` |
| Migración `AddPrioridad` | ✅ | `defaultValue: 1` (Media) ajustado a mano — EF scaffoldea 0 por default, no el valor del inicializador de C# |
| Orden automático por prioridad | ✅ | `TodoListColumnViewModel.SortItems` reordena `Items`/`CompletedItems` in-place (Alta→Media→Baja, luego por fecha de creación) al agregar, mover de bucket, o cambiar `Prioridad` |
| Badge de prioridad en la tarjeta | ✅ | Mismo patrón visual que el de `Estado` (gris/celeste/naranja) |
| Búsqueda global | ✅ | `MainViewModel.SearchText` + `TaskSearchVisibilityConverter` (primer converter del proyecto) filtran tarjetas por título/descripción en tiempo real |
| Tests | ✅ | 71/71 en verde (11 nuevos: `TaskSearchMatcherTests` + orden por prioridad en `MainViewModelTests`) |
| Verificación manual en la app | ⬜ | Pendiente — cambiar prioridades y confirmar el reordenamiento, buscar por título/descripción, confirmar que la búsqueda no encuentra por contenido de subtareas |

**Decisiones tomadas para este sprint** (ver [SPRINT5.md](../SPRINT5.md)): el orden por
prioridad es automático y fijo (no hay selector de criterio ni reordenamiento manual); las
subtareas no tienen prioridad propia ni participan del orden; la búsqueda solo mira
título/descripción de tareas de primer nivel, nunca subtareas; las columnas nunca se
ocultan por la búsqueda, solo sus tarjetas.

---

## ✅ Testing

**Estado:** Suite inicial de unit tests agregada (proyecto `ToDoApp.Tests`, xUnit).

| Área | Cobertura | Detalles |
|---|---|---|
| **Modelo** (`TodoItem`) | ✅ | Sincronización `IsDone`↔`Estado`, notificación `INotifyPropertyChanged` |
| **Servicio** (`TodoService`) | ✅ | CRUD completo contra EF Core InMemory (no toca SQLite real) |
| **Comandos** (`RelayCommand`/`RelayCommand<T>`) | ✅ | `CanExecute`, `Execute`, `RaiseCanExecuteChanged` |
| **ViewModel** (`MainViewModel`, `TodoListColumnViewModel`) | ✅ | Carga inicial agrupada por lista, alta/borrado de listas y tareas, mover-vs-eliminar al borrar lista |
| **Servicio** (`TodoListService`) | ✅ | CRUD de listas, borrado predeterminado bloqueado, mover/eliminar tareas |
| **Servicio** (`TodoService`, subtareas) | ✅ | `AddSubTaskAsync`/`GetSubTasksAsync`, límite de 1 nivel, herencia de lista, cascada al borrar el padre |
| **ViewModel** (subtareas) | ✅ | Árbol armado al cargar, alta/borrado de subtarea, persistencia con la lista correcta |
| **Servicio** (`BackupService`) | ✅ | Export con jerarquía anidada, import como reemplazo total, fallback de lista predeterminada, límite de 1 nivel, round-trip |
| **Búsqueda** (`TaskSearchMatcher`) | ✅ | Match case-insensitive por título/descripción, vacío = sin filtro |
| **ViewModel** (orden por prioridad) | ✅ | `Items`/`CompletedItems` ordenados Alta→Media→Baja al agregar, mover de bucket, o cambiar `Prioridad` |
| Integration tests (UI/E2E) | ⬜ | No implementado |

```powershell
# Correr toda la suite
dotnet test ToDoApp/ToDoApp.slnx
```

71 tests, todos en verde al momento de este commit (27 de Sprint 1 + 10 de Sprint 2 + 8 de Sprint 3 + 7 de mover tareas entre listas + 8 de Sprint 4/backup + 11 nuevos de Sprint 5/búsqueda-prioridad).

---

## ✨ Funcionalidades Disponibles

| Funcionalidad | Estado | Nota |
|---|---|---|
| Crear tarea | ✅ | Con título y descripción |
| Mostrar tareas | ✅ | Cargadas del store |
| Marcar como completada | ✅ | Via checkbox |
| Editar título/descripción | ⚠️ | Parcial - Requiere re-edit |
| Eliminar tarea | ✅ | Con persistencia |
| Persistencia SQLite | ✅ | Automática |
| Carga al iniciar | ✅ | Async en Grid_Loaded |
| Validaciones básicas | ✅ | No permite vacíos |
| Operaciones async | ✅ | Implementadas |

---

## ⚠️ Funcionalidades Pendientes

### Alta Prioridad

- [ ] **Edición en línea** - Permitir editar título/descripción de tareas existentes
- [ ] **Campos de fecha** - UI para FechaCreacion y FechaVencimiento
- [ ] **Estados visuales** - Mostrar estado (Pendiente/Completado/Cancelado)
- [ ] **Validaciones mejoradas** - Mensajes de error descriptivos

### Prioridad Media

- [x] **Categorías/Listas** - Organizar tareas por listas (Sprint 2)
- [x] **Prioridades** - Asignar nivel de urgencia (Sprint 5, pendiente verificación manual)
- [x] **Búsqueda** - Filtrar tareas por texto (Sprint 5, pendiente verificación manual)
- [x] **Ordenamiento** - Automático por prioridad dentro de cada columna (Sprint 5); no hay selector de criterio (fecha, manual, etc.) — ver "Fuera de alcance" en SPRINT5.md
- [ ] **Paginación** - Si hay muchas tareas

### Prioridad Baja

- [x] **Migraciones EF Core** - Implementado (`InitialCreate` + `Database.Migrate()`)
- [x] **Unit Tests** - Suite inicial en `ToDoApp.Tests` (modelo, servicio, comandos, ViewModel)
- [ ] **Integration Tests** - Pruebas de integración
- [ ] **Subtareas** - Jerarquía de tareas
- [ ] **Recurrencia** - Tareas repetidas
- [ ] **Recordatorios** - Notificaciones
- [ ] **Exportación** - CSV, JSON, etc.

---

## 🚨 Riesgos Actuales

| Riesgo | Impacto | Mitigación |
|---|---|---|
| Edición en línea limitada | Bajo | Agregar UI de edición completa |
| Base de datos sin backups | Alto | Autobackup o exportación |
| Escalabilidad | Bajo | Considerar para sprints futuros |
| UI básica | Bajo | Mejorar styling y UX |
| Sin integration/E2E tests | Bajo | Cubierto por unit tests por ahora; agregar en sprints futuros |

**Resueltos en este ciclo:**
- ~~Schema desactualizado rompía la app (`EnsureCreated()` no aplicaba cambios de modelo)~~ → migraciones EF Core.
- ~~Sin unit tests~~ → 27 tests cubriendo modelo, servicio, comandos y ViewModel.
- ~~Vulnerabilidad alta en `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 (CVE-2025-6965)~~ → pineada a 2.1.12.

---

## 📈 Avance por Sprint

| Sprint | Completitud | Estado | Nota |
|---|---|---|---|
| **Sprint 0** | 100% | ✅ Completado | Arquitectura base lista |
| **Sprint 1** | 100% | ✅ Completado | CRUD, edición en línea, estados visuales, validaciones |
| **Sprint 2** | 100% | ✅ Completado | Listas/board implementado y verificado manualmente |
| **Sprint 3** | 100% | ✅ Completado | Subtareas jerárquicas (un nivel), verificado manualmente |
| **Sprint 4** | 100% | ✅ Completado | Backup/export/import, verificado manualmente |
| **Total Proyecto (MVP)** | 100% | ✅ **MVP completo** | Sprints 0-4 completados y verificados |
| **Sprint 5** (post-MVP) | ~90% | 🟡 Implementado, falta verificación manual | Búsqueda y prioridad |

---

## 🎯 Próximas Tareas Priorizadas

### Fase Actual (Sprint 1 - Finalización)

**Estimación:** 2-3 días

1. ✅ [COMPLETADO] Implementar `TodoService.Update()`
2. ✅ [COMPLETADO] Persistir cambios de `IsDone`
3. ✅ [COMPLETADO] Implementar `TodoService.Delete()`
4. ✅ [COMPLETADO] Agregar `DeleteTaskCommand`
5. 🟡 [EN PROGRESO] Edición de tareas en UI
6. 🟡 [EN PROGRESO] Mostrar campos de fecha

### Fase Sprint 2 (Organización)

**Estimación:** 3-4 días

7. Crear modelo `TodoList`
8. Implementar CRUD de listas
9. Asociar tareas a listas
10. UI para gestión de listas

### Fase Sprint 3 (Jerarquía)

**Estimación:** 4-5 días. Plan detallado en [SPRINT3.md](SPRINT3.md).

11. Auto-referencia en TodoItem (subtareas)
12. UI for nested tasks
13. Persistencia de jerarquía

---

## 🔄 Proceso de Actualización

Este documento debe actualizarse en cada commit significativo:

**Checklist de actualización:**

- [ ] Cambiar fecha en header
- [ ] Actualizar tabla de CRUD si corresponde
- [ ] Actualizar porcentaje de Sprint
- [ ] Mover tareas completadas a ✅
- [ ] Agregar nuevas tareas pendientes
- [ ] Actualizar avance estimado
- [ ] Commit con mensaje: `docs: Update PROJECT_STATUS.md`

---

## 📝 Notas de Desarrollo

### Ambiente de Desarrollo

```
IDE:           Microsoft Visual Studio Community 2026 (18.7.0-insiders)
SDK:           .NET (WPF)
Terminal:      PowerShell
Repositorio:   https://github.com/COPY0096/ToDo_App
Rama:          main
```

### Compilación

```powershell
# Build Debug
msbuild ToDoApp/ToDoApp.csproj /p:Configuration=Debug

# Build Release
msbuild ToDoApp/ToDoApp.csproj /p:Configuration=Release

# Limpiar
msbuild ToDoApp/ToDoApp.csproj /t:Clean
```

### Base de Datos

```
Motor:    SQLite
Archivo:  Local (auto-creado en app directory)
Creación: Migraciones EF Core (Database.Migrate() al iniciar)
Encoding: UTF-8
```

### Migraciones EF Core

Cualquier cambio a `TodoItem` (o al `DbContext`) requiere generar una nueva migración antes de que la app la levante:

```powershell
cd ToDoApp
dotnet ef migrations add <NombreDescriptivo> -o Data/Migrations
```

`App.xaml.cs` aplica las migraciones pendientes automáticamente al arrancar (`Database.Migrate()`), tanto en una DB nueva como en una existente.

---

## 📞 Contacto y Colaboración

**Desarrolladores:**
- COPY0096 (Github)

**Comunicación:**
- Issues en GitHub para bugs
- Projects para tracking
- PRs con código

**Convenciones de commits:**
```
feat:  Nueva funcionalidad
fix:   Correción de bugs
docs:  Actualización de documentación
refactor: Reestructuración de código
test: Agregación de tests
```

---

## 📚 Referencias Útiles

- [WPF Documentation](https://learn.microsoft.com/dotnet/desktop/wpf/)
- [MVVM Pattern](https://learn.microsoft.com/dotnet/architecture/maui/mvvm)
- [Entity Framework Core](https://learn.microsoft.com/ef/core/)
- [SQLite Official](https://www.sqlite.org/)

---

**Estado:** 🎉 **MVP completo** (Sprints 0-4, todos verificados manualmente). Encima del MVP,
Sprint 5 (búsqueda y prioridad) ya está implementado con 71/71 tests en verde; falta la
verificación manual del usuario en la app real para darlo por cerrado. Lo que queda después
es backlog post-MVP deliberadamente diferido — sync en la nube, versionado de backups,
selector de criterio de orden, integración con Google Tasks, etc. (ver las secciones "Fuera
de alcance" de cada SPRINTx.md y "Funcionalidades Pendientes" más abajo).

