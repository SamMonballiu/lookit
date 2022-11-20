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
            var measureDistance = clickedDistance * scale.Factor;
            return measureDistance;
        }

        public void Straighten(int maxDistance)
        {
            Func<int, int, bool> AreNear = (first, second) => Math.Abs(first - second) < maxDistance;

            if (AreNear(Start.X, End.X))
            {
                Points.RemoveAt(1);
                Points.Insert(1, new Point(Start.X, End.Y));
            }

            if (AreNear(Start.Y, End.Y))
            {
                Points.RemoveAt(1);
                Points.Insert(1, new Point(End.X, Start.Y));
            }
        }

        public override string ToString() => $"({Start.X},{Start.Y}) - ({End.X},{End.Y}) Dist: {Math.Round(Distance, 2)}";
    }
}
