using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace DotnetDumpMonitor.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public sealed class BoolInvertConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (!(value is bool))
                return null;
            return (bool)value ? false : true;
        }

        public object? ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (Equals(value, false))
                return true;
            if (Equals(value, true))
                return false;
            return null;
        }
    }
}
