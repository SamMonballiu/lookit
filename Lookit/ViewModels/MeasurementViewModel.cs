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

        [ObservableProperty, NotifyPropertyChangedFor(nameof(Value))]
        protected Scale _scale;

        public Measurement Measurement { get; }

        public MeasurementViewModel(Measurement measurement, Scale scale)
        {
            Measurement = measurement;
            _scale = scale;
            ScaleContext.OnScaleChanged += newScale => _scale = newScale;
        }
    }

    public partial class PolygonMeasurementViewModel : MeasurementViewModel
    {
        public string Points => string.Join(",", Measurement.Points.Select(p => $"{p.X}, {p.Y}"));
        public System.Windows.Point Center => GetCenter();

        public override string Value => $"{Math.Abs((Measurement as PolygonalMeasurement).GetScaledArea(_scale) ?? 0):F} {Scale.Unit.ToSquaredString()}";

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

            return new System.Windows.Point(centerX, centerY);
        }

        public PolygonMeasurementViewModel(Measurement measurement, Scale scale) : base(measurement, scale) { }
    }

    public partial class LineMeasurementViewModel : MeasurementViewModel
    {
        public override string Value => ((Measurement as LineMeasurement).GetScaledDistance(_scale) ?? 0).ToString("F");

        public LineMeasurementViewModel(LineMeasurement measurement, Scale scale)
            : base(measurement, scale) { }
    }
}
