using Microsoft.UI.Xaml.Data;
using System;

namespace AfishaUno.Presentation.Converters
{
    public class NullableDateTimeToDateTimeOffsetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                // При отсутствии даты возвращаем текущую дату
                // Это необходимо, т.к. DatePicker не может отображать null
                return DateTimeOffset.Now;
            }
            
            if (value is DateTime dateTime)
            {
                return new DateTimeOffset(dateTime);
            }
            else if (value is DateTimeOffset dateTimeOffset)
            {
                return dateTimeOffset;
            }
            
            // Если не удалось преобразовать, возвращаем текущую дату
            return DateTimeOffset.Now;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return null;
            }
            
            if (value is DateTimeOffset dateTimeOffset)
            {
                // Проверяем, является ли это сегодняшней датой (предполагаем, что пользователь ее не выбирал)
                bool isDefaultDate = Math.Abs((dateTimeOffset.Date - DateTime.Now.Date).TotalDays) < 1;
                
                if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
                {
                    // Если это текущая дата (по умолчанию), возвращаем null для DateTime?
                    return isDefaultDate && targetType == typeof(DateTime?) ? null : dateTimeOffset.DateTime;
                }
                else if (targetType == typeof(DateTimeOffset) || targetType == typeof(DateTimeOffset?))
                {
                    // Если это текущая дата (по умолчанию), возвращаем null для DateTimeOffset?
                    return isDefaultDate && targetType == typeof(DateTimeOffset?) ? null : dateTimeOffset;
                }
            }
            
            return null;
        }
    }
} 