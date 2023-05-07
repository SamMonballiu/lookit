using Lookit.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Lookit.ValueConverters
{
    internal class MeasurementsTypeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var measurements = (ObservableCollection<MeasurementViewModel>)values[0];
            var type = (string)values[1];

            return type switch
            {
                "Lines" => measurements.OfType<LineMeasurementViewModel>(),
                "Polygons" => measurements.OfType<PolygonMeasurementViewModel>(),
                _ => measurements
            };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
