using Lookit.Logic;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Lookit.ValueConverters
{
    internal class LinePreviewStrokeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mode = (Mode)value;

            return mode switch
            {
                Mode.MeasureLine => Brushes.CornflowerBlue,
                Mode.Scale => Brushes.Yellow,
                _ => Brushes.Transparent,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
