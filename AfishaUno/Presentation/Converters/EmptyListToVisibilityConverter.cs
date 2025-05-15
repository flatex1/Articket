using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace AfishaUno.Presentation.Converters
{
    /// <summary>
    /// Конвертер для отображения элемента, если список пуст
    /// </summary>
    public class EmptyListToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int count)
            {
                return count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
} 