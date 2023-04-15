using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Lookit.ValueConverters
{

    internal class LineLabelPositionConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var start = new Point((double)values[0], (double)values[1]);
                var end = new Point((double)values[2], (double)values[3]);

                if (start is { X: 0, Y: 0} && end is { X: 0, Y: 0})
                {
                    return new Thickness(int.MaxValue);
                }

                var middle = new Point(System.Convert.ToInt32((start.X + end.X) / 2), System.Convert.ToInt32((start.Y + end.Y) / 2));

                var labelWidth = (double)values[4];
                var labelHeight = (double)values[5];

                return new Thickness(middle.X - (labelWidth / 2), middle.Y - (labelHeight / 2), 0, 0);
            }
            catch
            {
                return new Thickness(int.MaxValue);
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
