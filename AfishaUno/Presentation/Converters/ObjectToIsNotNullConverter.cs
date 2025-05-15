using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace AfishaUno.Presentation.Converters
{
    public class ObjectToIsNotNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // Проверяем, какой тип ожидается на выходе
            if (targetType == typeof(Visibility))
            {
                return value != null ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                // Для Boolean и других типов возвращаем bool
                return value != null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
