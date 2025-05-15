using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace AfishaUno.Presentation.Converters
{
    public class CategoryToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string category && parameter is string targetCategory)
            {
                return category == targetCategory ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
} 