using Lookit.Context;
using Lookit.Models;
using MvvmHelpers;
using System;
using System.Linq;

namespace Lookit.ViewModels
{
    public class PolygonMeasurementViewModel: ObservableObject
    {
        public PolygonalMeasurement Measurement { get; }
        public string Points => string.Join(",", Measurement.Points.Select(p => $"{p.X}, {p.Y}"));
        private Scale _scale;

        public Scale Scale
        {
            get => _scale;
            set
            {
                SetProperty(ref _scale, value);
                OnPropertyChanged(nameof(Area));
            }
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

    public class LineMeasurementViewModel: ObservableObject
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
