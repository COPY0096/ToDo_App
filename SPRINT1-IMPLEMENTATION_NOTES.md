Implementing: Date picker UI for FechaCreacion and FechaVencimiento

Planned changes:
- Add DatePicker controls to MainWindow.xaml for FechaCreacion (read-only) and FechaVencimiento (editable).
- Update TodoItem view model properties and bindings if needed.
- Ensure TodoService persists FechaVencimiento changes and DateTime is stored in SQLite.
- Add input validation for FechaVencimiento (cannot be before FechaCreacion).
- Run unit tests and UI smoke test.

Next step: open MainWindow.xaml and ViewModel files, apply changes, run build/tests.