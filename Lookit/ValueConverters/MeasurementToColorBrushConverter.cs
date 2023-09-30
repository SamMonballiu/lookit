using Lookit.Context;
using Lookit.Models;
using Lookit.ViewModels;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Lookit.ValueConverters
{
    public class MeasurementToColorBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var viewmodel = (MeasurementViewModel)values[0];
                var selectedMeasurement = (Measurement)values[1];

                return viewmodel.Measurement.Equals(selectedMeasurement) ? Brushes.Aqua : Brushes.Red;
            }
            catch
            {
                return Brushes.Pink;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}