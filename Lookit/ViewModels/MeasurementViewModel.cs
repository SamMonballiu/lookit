using Lookit.Context;
using Lookit.Models;
using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Lookit.Extensions;

namespace Lookit.ViewModels
{
    public abstract partial class MeasurementViewModel : ObservableObject
    {
        public abstract string Value { get; }
        public abstract string Summary { get; }

        [ObservableProperty, NotifyPropertyChangedFor(nameof(Value)), NotifyPropertyChangedFor(nameof(Summary))]
        protected Scale _scale;

        [ObservableProperty]
        protected string _name;

        [ObservableProperty]
        protected bool _hidden;

        public Measurement Measurement { get; }

        public MeasurementViewModel(Measurement measurement, Scale scale, string name)
        {
            Measurement = measurement;
            _scale = scale;
            _name = name;
            ScaleContext.OnScaleChanged += newScale =>
            {
                _scale = newScale;
                OnPropertyChanged(nameof(Value));
            };
        }
    }

    public partial class PolygonMeasurementViewModel : MeasurementViewModel
    {
        public string Points => string.Join(",", Measurement.Points.Select(p => $"{p.X}, {p.Y}"));
        public System.Windows.Point Center => GetCenter();

        public override string Value => _scale.IsDefault
            ? "-"
            : $"{Math.Abs((Measurement as PolygonalMeasurement).GetScaledArea(_scale) ?? 0):F} {Scale.Unit.ToSquaredString()}";

        public string Perimeter => _scale.IsDefault ? "-" : $"{GetPerimeter():F} {Scale.Unit.ToUnitString()}";

        public override string Summary => $"{Value} / {Perimeter}";


        private System.Windows.Point GetCenter()
        {
            if (!Measurement.Points.Any())
            {
                return new System.Windows.Point(int.MaxValue, int.MaxValue);
            }

            var byX = Measurement.Points.OrderBy(pt => pt.X);
            var byY = Measurement.Points.OrderBy(pt => pt.Y);

            var leftMost = byX.First();
            var rightMost = byX.Last();
            var highest = byY.First();
            var lowest = byY.Last();

            var centerX = leftMost.X + (rightMost.X - leftMost.X) / 2;
            var centerY = highest.Y + (lowest.Y - highest.Y) / 2;

            return new System.Windows.Point(centerX, centerY);
        }

        private double GetPerimeter()
        {
            double result = 0;
            for (int i = 0; i <= Measurement.Points.Count - 1; i++)
            {
                var current = Measurement.Points[i];
                var nextIndex = current == Measurement.Points.Last()
                    ? 0
                    : i + 1;
                var next = Measurement.Points[nextIndex];
                var line = new LineMeasurement(current, next);
                var distance = line.GetScaledDistance(Scale) ?? 0;
                System.Diagnostics.Debug.WriteLine($"{result} + {distance} = {result+distance}");
                result += distance;
            }

            return result;
        }

        public PolygonMeasurementViewModel(Measurement measurement, Scale scale, string name) : base(measurement, scale, name) { }

        public System.Drawing.Point Origin => Measurement.Points.Any() ? Measurement.Points.First() : System.Drawing.Point.Empty;
    }

    public partial class LineMeasurementViewModel : MeasurementViewModel
    {
        public override string Value => _scale.IsDefault 
            ? "-"
            : $"{(Measurement as LineMeasurement).GetScaledDistance(_scale) ?? 0:F} {Scale.Unit.ToShortString()}";

        public override string Summary => Value;

        public LineMeasurementViewModel(LineMeasurement measurement, Scale scale, string name)
            : base(measurement, scale, name) { }
    }
}
