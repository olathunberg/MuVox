using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RecordToMP3.UI_Features.Converters
{
    public class TenthSecondToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            var secondsRecorded = (int)value;
            return string.Format("{0:00}:{1:00}.{2:0}", (secondsRecorded / 10) / 60, (secondsRecorded / 10) % 60, secondsRecorded % 10);
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
            // Do the conversion from visibility to bool
        }
    }
}
