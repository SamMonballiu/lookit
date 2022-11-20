using Lookit.Context;
using Lookit.Models;
using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Lookit.ViewModels
{
    public abstract partial class MeasurementViewModel : ObservableObject
    {
        public abstract string Value { get; }

        [ObservableProperty, NotifyPropertyChangedFor(nameof(Value))]
        protected Scale _scale;

        public Measurement Measurement { get; }

        public MeasurementViewModel(Measurement measurement)
        {
            Measurement = measurement;
        }

    }

    public partial class PolygonMeasurementViewModel : MeasurementViewModel
    {
        public string Points => string.Join(",", Measurement.Points.Select(p => $"{p.X}, {p.Y}"));
        private Scale _scale;
        public System.Windows.Point Center => GetCenter();

        public override string Value => $"{Math.Abs((Measurement as PolygonalMeasurement).GetScaledArea(_scale) ?? 0).ToString("F")} {Scale.Unit.ToSquaredString()}";

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

        private PolygonMeasurementViewModel(PolygonalMeasurement measurement, Scale scale)
            : base(measurement)
        {
            Scale = scale;
            ScaleContext.OnScaleChanged += newScale => Scale = newScale;
        }

        public static PolygonMeasurementViewModel From(PolygonalMeasurement measurement, Scale scale) => new PolygonMeasurementViewModel(measurement, scale);
    }

    public partial class LineMeasurementViewModel : MeasurementViewModel
    {
        public override string Value => ((Measurement as LineMeasurement).GetScaledDistance(_scale) ?? 0).ToString("F");

        public LineMeasurementViewModel(LineMeasurement measurement, Scale scale)
            : base(measurement)
        {
            Scale = scale;
            ScaleContext.OnScaleChanged += newScale => Scale = newScale;
        }

        public static LineMeasurementViewModel From(LineMeasurement measurement, Scale scale) => new LineMeasurementViewModel(measurement, scale);
    }
}
