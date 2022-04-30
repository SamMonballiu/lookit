using System;
using System.Drawing;

namespace Lookit.Models
{
    public class LineMeasurement
    {
        private Point _first;
        private Point _second;

        public LineMeasurement(Point first, Point second)
        {
            _first = first;
            _second = second;
        }

        private double GetDistance(Point first, Point second)
            => Math.Sqrt((Math.Pow(first.X - second.X, 2) + Math.Pow(first.Y - second.Y, 2)));

        public double GetScaledDistance(Scale scale)
        {
            var clickedDistance = GetDistance(_first, _second);
            var measureDistance = clickedDistance * scale.DistanceUnit;
            return measureDistance;
        }

        public override string ToString() => $"({_first.X},{_first.Y}) - ({_second.X},{_second.Y})";
    }
}
