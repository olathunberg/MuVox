using System;
using System.Globalization;
using System.Windows.Data;

namespace TTech.Muvox.UI_Features.Converters
{
    public class TenthSecondToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;
            var secondsRecorded = (int)value;
            if (secondsRecorded < 0)
                throw new ArgumentOutOfRangeException();

            return string.Format("{0:00}:{1:00}.{2:0}", (secondsRecorded / 10) / 60, (secondsRecorded / 10) % 60, secondsRecorded % 10);
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
