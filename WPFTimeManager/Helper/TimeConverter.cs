using System;
using System.Windows.Data;

namespace WPFTimeManager
{

    [ValueConversion(typeof(long), typeof(string))]
    public class TimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Возвращаем строку в формате 123.456.789 руб.
            long var = (long)value;
            return (var / 1440).ToString().PadLeft(2, '0') + ":" + ((var / 60) % 24).ToString().PadLeft(2, '0') + ":" + (var % 60).ToString().PadLeft(2, '0');
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //TODO реализовать обратное преобразование
            return null;
        }
    }
}