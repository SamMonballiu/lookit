using Lookit.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Lookit.ValueConverters
{
    internal class MeasurementPointsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var measurements = (ObservableCollection<MeasurementViewModel>)value;
            var points = measurements
                .Where(m => !m.Hidden)
                .SelectMany(x => x.Measurement.Points).Select(pt => $"{pt.X},{pt.Y}");

            return points;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
