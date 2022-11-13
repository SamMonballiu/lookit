using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Lookit.ValueConverters
{
    internal class PointToThicknessConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var center = (Point)values[0];
            var textBlockWidth = (double)values[1];
            var textBlockHeight = (double)values[2];

            var left = center.X - (textBlockWidth / 2);
            var top = center.Y - (textBlockHeight / 2);
            return new Thickness(left, top, 0, 0);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
