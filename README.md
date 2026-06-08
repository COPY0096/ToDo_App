# ToDo Desktop App

## Descripción

ToDo Desktop App es una aplicación de escritorio desarrollada en WPF para la gestión de tareas personales mediante listas jerárquicas. Permite organizar actividades, crear subtareas ilimitadas, gestionar fechas, controlar estados y configurar tareas recurrentes.

El proyecto utiliza el patrón MVVM para mantener una arquitectura desacoplada, escalable y fácil de mantener.

---

## Objetivo del Proyecto

Desarrollar una solución de productividad personal que permita administrar tareas complejas mediante estructuras jerárquicas, recordatorios y reglas de recurrencia, almacenando toda la información localmente.

---

## Tecnologías Utilizadas

### Frontend

* WPF (.NET)
* XAML

### Arquitectura

* MVVM (Model-View-ViewModel)

### Backend Local

* Entity Framework Core
* SQLite

### Inyección de Dependencias

* Microsoft.Extensions.Hosting
* Microsoft.Extensions.DependencyInjection

---

## Funcionalidades Principales

### Gestión de Listas

* Crear listas
* Editar listas
* Eliminar listas
* Visualizar listas disponibles

### Gestión de Tareas

* Crear tareas
* Editar tareas
* Eliminar tareas
* Marcar tareas como completadas
* Reabrir tareas completadas
* Agregar descripción
* Asignar fecha de inicio
* Asignar fecha límite

### Subtareas Jerárquicas

* Crear subtareas dentro de una tarea
* Profundidad ilimitada
* Estado independiente por subtarea
* Fechas independientes por subtarea

### Recurrencia

* Diaria
* Semanal
* Mensual
* Anual
* Cada X días
* Cada X semanas
* Cada X meses
* Cada X años
* Cuotas de ejecución

### Organización

* Estado pendiente
* Estado en progreso
* Estado completada
* Filtros por lista
* Filtros por estado
* Filtros por fecha
* Búsqueda por texto
* Visualización de tareas vencidas

### Persistencia

* Base de datos SQLite local
* Guardado automático
* Carga automática al iniciar

---

## Reglas de Negocio

### Listas

* Una lista puede contener múltiples tareas.
* Eliminar una lista elimina todas sus tareas asociadas.

### Tareas

* Una tarea puede contener múltiples subtareas.
* Las tareas completadas pueden reabrirse.
* Las fechas deben mantener coherencia entre inicio y vencimiento.

### Recurrencia

* Las tareas recurrentes generan nuevas instancias automáticamente según su configuración.
* El historial de ejecuciones debe conservarse.

---

## Arquitectura del Proyecto

```text
ToDoApp
│
├── Models
│   ├── TaskItem
│   ├── TaskList
│   └── RecurrenceRule
│
├── ViewModels
│   ├── MainViewModel
│   ├── TaskViewModel
│   └── ListViewModel
│
├── Views
│   ├── MainWindow
│   ├── TaskView
│   └── ListView
│
├── Services
│   ├── TaskService
│   ├── RecurrenceService
│   └── NotificationService
│
├── Data
│   ├── AppDbContext
│   └── Migrations
│
└── App.xaml
```

---

## Estado Actual del MVP

### Completado

* Estructura inicial del proyecto
* Patrón MVVM
* Configuración de WPF
* Integración con SQLite
* Configuración de Entity Framework Core
* Configuración de Dependency Injection
* Creación de modelos base

### En Desarrollo

* CRUD de listas
* CRUD de tareas
* Gestión de subtareas
* Persistencia completa
* Filtros y búsquedas

### Pendiente

* Motor de recurrencia
* Recordatorios
* Notificaciones
* Configuración avanzada

---

## Requisitos

* Windows 10 o superior
* .NET SDK 9 o superior
* Visual Studio 2026 o superior

---

## Instalación

```bash
git clone <repositorio>

cd ToDoApp

dotnet restore

dotnet build

dotnet run
```

---

## Roadmap

### Sprint 0 — Preparación

* Configuración inicial
* Arquitectura MVVM
* SQLite
* Entity Framework Core

### Sprint 1 — Gestión de Listas

* CRUD de listas

### Sprint 2 — Gestión de Tareas

* CRUD de tareas

### Sprint 3 — Subtareas

* Árbol jerárquico

### Sprint 4 — Recurrencia

* Motor de repetición

### Sprint 5 — Filtros y Búsquedas

* Consultas rápidas

### Sprint 6 — Recordatorios

* Notificaciones locales

### Sprint 7 — Optimización y Pruebas

* Validación funcional
* Corrección de errores

---

## Futuras Integraciones

* Integración con Google Tasks
* Sincronización en la nube
* Exportación e importación de datos
* Aplicación móvil complementaria
* Autenticación de usuarios
* Sincronización multiplataforma

---

## Licencia

Proyecto académico y de aprendizaje.
