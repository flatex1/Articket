using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace AfishaUno.Presentation.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isInverse = parameter is string paramStr && paramStr.Equals("Inverse", StringComparison.OrdinalIgnoreCase);
            bool result = false;
            // Если значение - bool, преобразуем его напрямую
            if (value is bool boolValue)
            {
                result = boolValue;
            }
            
            // Если значение - число, проверяем > 0
            else if (value is int intValue)
            {
                result = intValue > 0;
            }
            
            // Для других числовых типов
            else if (value is double doubleValue)
            {
                result = doubleValue > 0;
            }
            
            else if (value is float floatValue)
            {
                result = floatValue > 0;
            }
            
            else if (value is long longValue)
            {
                result = longValue > 0;
            }
            
            // Для нулевых значений - скрываем
            else if (value == null)
            {
                result = false;
            }
            
            // По умолчанию - отображаем
            else
            {
                result = true;
            }
            
            if (isInverse)
                result = !result;
            return result ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            // Если нужно преобразование в обратную сторону
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }
            
            return false;
        }
    }
} 