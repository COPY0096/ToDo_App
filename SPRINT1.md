Sprint 1 - Gestión de Tareas
===========================

Objetivo: CRUD básico de tareas con persistencia en SQLite.

Entregables:
- Modelo TodoItem con campos Id, Title, Description, FechaCreacion, FechaVencimiento, Estado.
- Servicios: TodoService usando Entity Framework Core + SQLite (AppDbContext).
- ViewModel: MainViewModel supports Add, Delete, Toggle complete and loads items at startup.
- UI: MainWindow XAML allows creating tasks (title + description), listing tasks, toggling complete, and deleting.

Manual verification:
1. Run the app.
2. Create a new task with title and description.
3. Close the app and re-open it; the task should persist.
4. Toggle the checkbox to mark complete; changes persist.
5. Delete a task to remove it from the database.
