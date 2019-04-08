using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            return string.Format("{0:00}:{1:00}.{2:0}", (secondsRecorded / 10) / 60, (secondsRecorded / 10) % 60, secondsRecorded % 10);
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
#pragma warning disable RECS0083 // Shows NotImplementedException throws in the quick task bar
            throw new NotImplementedException();
#pragma warning restore RECS0083 // Shows NotImplementedException throws in the quick task bar
            // Do the conversion from visibility to bool
        }
    }
}
