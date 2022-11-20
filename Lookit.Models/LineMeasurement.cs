using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace Lookit.Models
{
    public class LineMeasurement : Measurement
    {
        public Point Start => Points.First();
        public Point End => Points.Last();
        public double Distance => GetDistance(Start, End);
        public LineMeasurement(Point first, Point second)
        {
            Points = new List<Point>() { first, second };
        }

        private double GetDistance(Point first, Point second)
            => Math.Sqrt((Math.Pow(first.X - second.X, 2) + Math.Pow(first.Y - second.Y, 2)));

        public double? GetScaledDistance(Scale scale)
        {
            if (scale is null)
            {
                return null;
            }

            var clickedDistance = GetDistance(Start, End);
            var scaledDistance = clickedDistance * scale.Factor;
            return scaledDistance;
        }

        public override string ToString() => $"({Start.X},{Start.Y}) - ({End.X},{End.Y}) Dist: {Math.Round(Distance, 2)}";
    }
}
