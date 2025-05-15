using Microsoft.UI.Xaml.Data;

namespace AfishaUno.Presentation.Converters
{
    public class IndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int number)
            {
                return number - 1; // Преобразуем номер (1-based) в индекс (0-based)
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
} 