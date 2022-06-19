using Lookit.Context;
using Lookit.Models;
using MvvmHelpers;

namespace Lookit.ViewModels
{
    public class MeasurementViewModel: ObservableObject
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

        private MeasurementViewModel(LineMeasurement measurement, Scale scale)
        {
            Measurement = measurement;
            _scale = scale;
            ScaleContext.OnScaleChanged += newScale => Scale = newScale;
        }

        public static MeasurementViewModel From(LineMeasurement measurement, Scale scale) => new MeasurementViewModel(measurement, scale);
    }
}
