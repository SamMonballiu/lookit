using Lookit.Context;
using Lookit.Models;
using MvvmHelpers;
using System;
using System.Linq;

namespace Lookit.ViewModels
{
    public class PolygonMeasurementViewModel : ObservableObject
    {
        public PolygonalMeasurement Measurement { get; }
        public string Points => string.Join(",", Measurement.Points.Select(p => $"{p.X}, {p.Y}"));
        private Scale _scale;
        public System.Windows.Point Center => GetCenter();

        public Scale Scale
        {
            get => _scale;
            set
            {
                SetProperty(ref _scale, value);
                OnPropertyChanged(nameof(Area));
            }
        }

        private System.Windows.Point GetCenter()
        {
            var byX = Measurement.Points.OrderBy(pt => pt.X);
            var byY = Measurement.Points.OrderBy(pt => pt.Y);

            var leftMost = byX.First();
            var rightMost = byX.Last();
            var highest = byY.First();
            var lowest = byY.Last();

            var centerX = leftMost.X + (rightMost.X - leftMost.X) / 2;
            var centerY = highest.Y + (lowest.Y - highest.Y) / 2;

            return new System.Windows.Point(
                    centerX, centerY
                );
        }

        public string Area => Math.Abs(Measurement.GetScaledArea(_scale) ?? 0).ToString("F");

        private PolygonMeasurementViewModel(PolygonalMeasurement measurement, Scale scale)
        {
            Measurement = measurement;
            _scale = scale;
            ScaleContext.OnScaleChanged += newScale => Scale = newScale;
        }

        public static PolygonMeasurementViewModel From(PolygonalMeasurement measurement, Scale scale) => new PolygonMeasurementViewModel(measurement, scale);
    }

    public class LineMeasurementViewModel : ObservableObject
    {
        public LineMeasurement Measurement { get; }
        private Scale _scale;

        public Scale Scale
        {
            get => _scale;
            set
            {
                SetProperty(ref _scale, value);
                OnPropertyChanged(nameof(ScaledDistance));
            }
        }

        public string ScaledDistance => (Measurement.GetScaledDistance(_scale) ?? 0).ToString("F");

        private LineMeasurementViewModel(LineMeasurement measurement, Scale scale)
        {
            Measurement = measurement;
            _scale = scale;
            ScaleContext.OnScaleChanged += newScale => Scale = newScale;
        }

        public static LineMeasurementViewModel From(LineMeasurement measurement, Scale scale) => new LineMeasurementViewModel(measurement, scale);
    }
}
