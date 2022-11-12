using Lookit.Context;
using Lookit.Models;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Lookit.ValueConverters
{
    public class PolygonalMeasurementToColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var selected = SelectedMeasurementContext.SelectedMeasurement;
            if (value is PolygonalMeasurement measurement && measurement.Equals(selected))
            {
                return Brushes.Red;
            }

            return Brushes.CornflowerBlue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}