# ToDo App — Estado Actual del Proyecto

**Última actualización:** 5 de septiembre de 2026

---

## 📋 Estado General

El proyecto corresponde a una aplicación de escritorio desarrollada con **WPF**, siguiendo el patrón **MVVM**, utilizando **Entity Framework Core**, **SQLite** y **Microsoft.Extensions.Hosting** para la inyección de dependencias.

**Estado de compilación:** ✅ Compila correctamente sin errores ni warnings

**Rama activa:** `main`

**Último commit:** Sprint 2 (organización en listas/board) en curso — ver sección propia más abajo.

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

**Estado:** 🟡 **EN PROGRESO** — modelo, migración, servicios, ViewModel y UI implementados; falta verificación manual en la app real (ver [SPRINT2.md](../SPRINT2.md)).

| Ítem | Estado | Detalles |
|---|---|---|
| Modelo `TodoList` | ✅ | `Id`, `Nombre`, `Orden`, `FechaCreacion`, `EsPredeterminada` |
| `TodoItem.TodoListId` (FK) | ✅ | Requerida; relación 1 lista → * tareas, `DeleteBehavior.Restrict` |
| Lista predeterminada "Mis Tareas" | ✅ | Sembrada vía `HasData` (Id=1); no eliminable |
| Migración `AddTodoLists` | ✅ | Backfill automático: tareas existentes del Sprint 1 quedan asignadas a "Mis Tareas" (`DEFAULT 1` en la columna nueva) |
| `TodoListService` | ✅ | CRUD de listas; `DeleteAsync` pregunta mover-vs-eliminar tareas, no es un comportamiento fijo |
| Board en `MainWindow.xaml` | ✅ | Columnas por lista, alta rápida "+ Add a task", sección "Completed (N)" colapsable, "+ Add new list" |
| Tests | ✅ | 37/37 en verde (`TodoListServiceTests` nuevo + `MainViewModelTests` reescrito para columnas) |
| Verificación manual en la app | ⬜ | Pendiente — correr `dotnet run` y revisar visualmente (ver checklist en SPRINT2.md) |

**Fuera de alcance de este sprint** (ver [SPRINT2.md](../SPRINT2.md) para el detalle): subtareas (Sprint 3), integración con Google Tasks, menú completo de lista (Sort/Share/Print/Export/papelera), etiquetas/adjuntos por tarea, drag & drop de columnas.

**Bug preexistente corregido de paso:** `TodoItem.FechaCreacion` no tenía setter, por lo que EF Core nunca la mapeaba a columna (no aparecía en el snapshot del modelo) y se recalculaba a "ahora" en cada carga desde la DB. Ahora tiene setter y se persiste correctamente; las tareas creadas antes de este fix quedan con una fecha centinela (`0001-01-01`) ya que su fecha real de creación nunca se guardó.

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
| Integration tests (UI/E2E) | ⬜ | No implementado |

```powershell
# Correr toda la suite
dotnet test ToDoApp/ToDoApp.slnx
```

37 tests, todos en verde al momento de este commit (27 de Sprint 1 + 10 nuevos de Sprint 2).

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

- [ ] **Categorías/Listas** - Organizar tareas por listas
- [ ] **Prioridades** - Asignar nivel de urgencia
- [ ] **Búsqueda** - Filtrar tareas por texto
- [ ] **Ordenamiento** - Por fecha, prioridad, etc.
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
| **Sprint 2** | ~85% | 🟡 En progreso | Listas/board implementado; falta verificación manual en la app |
| **Sprint 3** | 0% | ⬜ No iniciado | Subtareas jerárquicas |
| **Total Proyecto** | ~55% | 🟡 En progreso | Fase 2 de 4 casi completa |

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

**Estimación:** 4-5 días

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

**Estado:** El proyecto está en fase de desarrollo activo. Sprint 1 está casi completo con funcionalidad CRUD básica funcional, ahora respaldada por migraciones EF Core y una suite inicial de unit tests. Próximo paso: Sprint 2 con categorías/listas.

