using System;
using System.Drawing;

namespace Lookit.Models
{
    public class LineMeasurement
    {
        public Point Start { get; private set; }
        public Point End { get; private set; }
        public double Distance => GetDistance(Start, End);
        public LineMeasurement(Point first, Point second)
        {
            Start = first;
            End = second;
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
            var measureDistance = clickedDistance * scale.DistanceUnit;
            return measureDistance;
        }

        public void Straighten(int maxDistance)
        {
            Func<int, int, bool> AreNear = (first, second) => Math.Abs(first - second) < maxDistance;

            if (AreNear(Start.X, End.X))
            {
                End = new Point(Start.X, End.Y);
            }

            if (AreNear(Start.Y, End.Y))
            {
                End = new Point(End.X, Start.Y);
            }
        }

        public override string ToString() => $"({Start.X},{Start.Y}) - ({End.X},{End.Y}) Dist: {Math.Round(Distance, 2)}";
    }
}
