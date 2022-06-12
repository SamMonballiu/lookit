using Lookit.Models;

namespace Lookit.ViewModels
{
    public class MeasurementViewModel
    {
        public LineMeasurement Measurement { get; }
        private Scale _scale;

        public string ScaledDistance => (Measurement.GetScaledDistance(_scale) ?? 0).ToString("F");

        private MeasurementViewModel(LineMeasurement measurement, Scale scale)
        {
            Measurement = measurement;
            _scale = scale;
        }

        public static MeasurementViewModel From(LineMeasurement measurement, Scale scale) => new MeasurementViewModel(measurement, scale);
    }
}
