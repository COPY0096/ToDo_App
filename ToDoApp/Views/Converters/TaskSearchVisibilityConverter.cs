using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace ToDoApp.Views.Converters
{
    /// <summary>
    /// Primer converter del proyecto (Sprint 5): hasta ahora toda la UI condicional se
    /// resolvía con DataTrigger (comparación por igualdad), pero "contiene la subcadena X"
    /// no se puede expresar así. Values esperados: [Title, Description, SearchText].
    /// </summary>
    public class TaskSearchVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var title = values.ElementAtOrDefault(0) as string ?? string.Empty;
            var description = values.ElementAtOrDefault(1) as string ?? string.Empty;
            var searchText = values.ElementAtOrDefault(2) as string;

            return TaskSearchMatcher.Matches(title, description, searchText)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
