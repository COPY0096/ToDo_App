Sprint 4 - Confiabilidad de datos (Backup / Export / Import)
=============================================================

Objetivo: darle al usuario una forma de sacar sus datos de la app (backup
portable en un archivo) y de restaurarlos, cerrando el riesgo **Alto**
"Base de datos sin backups" que está anotado en PROJECT_STATUS.md desde el
Sprint 1 y nunca se resolvió. Sin esto, perder o corromper `todo.db` borra
todo sin posibilidad de recuperación.

Alcance: backup manual on-demand (exportar a un archivo JSON legible,
restaurar desde ese archivo) + una copia de seguridad automática liviana
como red de contención ante una migración fallida. **No** es un sistema de
sync en la nube ni de versionado de backups — ver "Fuera de alcance".

---

## Entregables

### 1. Formato de backup (DTOs nuevos)

El export es un árbol JSON auto-contenido, no un volcado de las tablas de
EF (evita exponer Ids internos que no tienen sentido en otra instalación,
y evita el problema de referencias circulares `TodoItem.TodoList` ↔
`TodoList.Items` / `TodoItem.ParentTask` ↔ `TodoItem.SubTasks` al
serializar con `System.Text.Json`).

Nuevo `Models/BackupData.cs`:

```csharp
public class BackupData
{
    public DateTime ExportedAt { get; set; }
    public List<TodoListBackup> Lists { get; set; } = new();
}

public class TodoListBackup
{
    public string Nombre { get; set; } = string.Empty;
    public int Orden { get; set; }
    public bool EsPredeterminada { get; set; }
    public List<TodoItemBackup> Tareas { get; set; } = new();
}

public class TodoItemBackup
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TodoEstado Estado { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public List<TodoItemBackup> SubTareas { get; set; } = new();
}
```

Los Ids de EF (`TodoList.Id`, `TodoItem.Id`, `TodoItem.TodoListId`,
`TodoItem.ParentTaskId`) **no** se exportan: al importar, SQLite asigna
Ids nuevos y las relaciones se reconstruyen por posición en el árbol
(lista → sus tareas → sus subtareas), igual que ya se arma la jerarquía en
memoria en `MainViewModel.InitializeAsync()`.

### 2. `BackupService` (nuevo)

```csharp
public class BackupService
{
    public async Task<BackupData> ExportAsync();
    public async Task ImportAsync(BackupData backup); // reemplaza TODOS los datos
}
```

- `ExportAsync`: recorre `TodoLists` ordenadas por `Orden`; para cada una,
  sus tareas de primer nivel (`ParentTaskId == null`) con
  `.Include(i => i.SubTasks)` explícito — no depende del fixup automático
  de EF que documenta SPRINT3.md, porque un backup tiene que ser correcto
  aunque algún día cambie la vida útil del `AppDbContext`.
- `ImportAsync`: **reemplazo total**, dentro de una transacción
  (`_db.Database.BeginTransactionAsync()`):
  1. Borra todas las filas de `TodoItems` y `TodoLists`.
  2. Inserta las listas del backup (en el orden dado), después sus tareas
     de primer nivel, después las subtareas de cada una.
  3. Si ninguna lista del backup viene marcada `EsPredeterminada` (archivo
     armado a mano, o corrupto), se marca la primera como predeterminada
     automáticamente — la app no puede quedar sin una lista predeterminada
     (`TodoListService.DeleteAsync` depende de que exista una).
  4. Si el backup no tiene ninguna lista, no hace nada (rechaza con
     `InvalidOperationException`, mismo estilo que el resto de los
     servicios) — importar un archivo vacío no debe dejar la app sin
     ninguna lista.

### 3. UI (XAML + code-behind)

- Dos botones nuevos junto al título "Main Board": **"Exportar backup"** y
  **"Restaurar backup"** (nombre elegido a propósito para dejar claro que
  reemplaza todo — ver Decisión 1).
- `Exportar backup` → `SaveFileDialog` (nombre sugerido
  `todo-backup-yyyyMMdd.json`) → `BackupService.ExportAsync()` →
  `JsonSerializer.Serialize(..., WriteIndented = true)` → escribir el
  archivo → `MessageBox` de confirmación con la ruta.
- `Restaurar backup` → `MessageBox` de advertencia primero ("Esto va a
  reemplazar TODOS los datos actuales por los del archivo. ¿Continuar?",
  Sí/No) → si confirma, `OpenFileDialog` → leer y deserializar → si el
  archivo es inválido (JSON corrupto, estructura inesperada), mostrar el
  error en un `MessageBox` con un mensaje descriptivo en vez de que la app
  se cuelgue o falle en silencio (mismo estándar que las validaciones del
  Sprint 1) → si es válido, `BackupService.ImportAsync(...)` y recargar el
  board (`MainViewModel` necesita un `ReloadAsync()` que limpie `Lists` y
  vuelva a correr la misma carga que `InitializeAsync()`).

### 4. Backup automático de seguridad (red de contención, no un backup real)

En `App.xaml.cs`, antes de `db.Database.Migrate()`: si `todo.db` ya existe,
copiarlo a `todo.db.bak` (sobrescribiendo el anterior — una sola copia
rotativa, no versionado). Cubre el caso "una migración nueva rompe algo":
siempre queda el estado de justo antes de esa migración. **No reemplaza**
al export manual como mecanismo real de backup/portabilidad — ver
Decisión 2.

---

## Fuera de alcance de este sprint (backlog futuro)

- **Sync en la nube** (Google Drive, OneDrive, etc.) — integración grande
  aparte, no es "backup local a un archivo".
- **Versionado de backups** (múltiples snapshots nombrados con historial)
  — por ahora es un export/import puntual bajo demanda, más el único
  `.bak` rotativo de la red de contención.
- **Export parcial** (una sola lista, o un rango de fechas) — el export
  siempre es del board completo para mantenerlo simple.
- **Merge al importar** (combinar con los datos existentes en vez de
  reemplazarlos) — deliberadamente fuera de alcance, ver Decisión 1.
- **Encriptar o proteger con contraseña el archivo de backup** — no hace
  falta para un backup local personal.

---

## Decisiones confirmadas

1. **Importar = reemplazo total, no merge.** El botón se llama
   "Restaurar backup" (no "Importar") para que quede inequívoco, y pide
   confirmación explícita antes de borrar los datos actuales.
2. **El `.bak` automático es solo una red de contención** ante una
   migración que rompa algo — una sola copia rotativa, sobrescrita en
   cada arranque. El mecanismo real de backup/portabilidad que el usuario
   controla es el export/import manual a JSON.
3. **Formato JSON legible** (indentado, sin comprimir/encriptar) para
   poder abrirlo e inspeccionarlo a mano si hace falta.

---

## Verificación manual

1. Crear algunas tareas y listas de prueba (con al menos una subtarea).
2. "Exportar backup" → guardar el archivo → abrirlo con un editor de texto
   y confirmar que se ve la estructura esperada (listas, tareas,
   subtareas, sin Ids).
3. Agregar una tarea más después de exportar (para notar la diferencia).
4. "Restaurar backup" con ese archivo → confirmar el diálogo de
   advertencia → verificar que el board vuelve exactamente al estado del
   momento del export (la tarea agregada después desaparece).
5. Intentar restaurar un archivo que no es un backup válido (ej. un
   `.json` cualquiera, o un `.txt`) → debe mostrar un error descriptivo,
   no romper la app.
6. Cerrar la app, editar manualmente `todo.db.bak` (o simplemente
   confirmar que existe) → reabrir la app y verificar que el `.bak` se
   actualiza a como estaba `todo.db` justo antes de este arranque.
