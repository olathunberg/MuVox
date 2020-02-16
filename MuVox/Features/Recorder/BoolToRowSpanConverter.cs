using System;
using System.Globalization;
using System.Windows.Data;

namespace TTech.MuVox.Features.Recorder
{
    public class BoolToRowSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return 1;

            if (value is bool monoDisplay && monoDisplay == true)
                return 3;

            return 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
