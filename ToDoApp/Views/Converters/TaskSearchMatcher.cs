using System;

namespace ToDoApp.Views.Converters
{
    /// <summary>
    /// Lógica pura del match de búsqueda (Sprint 5), separada del converter WPF
    /// para poder testearla directo sin pasar por IMultiValueConverter.
    /// </summary>
    public static class TaskSearchMatcher
    {
        /// <summary>
        /// True si <paramref name="searchText"/> está vacío/null (sin filtro activo),
        /// o si aparece en <paramref name="title"/> o <paramref name="description"/>
        /// (contains, case-insensitive). No mira subtareas — ver SPRINT5.md, Decisión 3.
        /// </summary>
        public static bool Matches(string title, string description, string? searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText)) return true;

            title ??= string.Empty;
            description ??= string.Empty;

            return title.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                || description.Contains(searchText, StringComparison.OrdinalIgnoreCase);
        }
    }
}
