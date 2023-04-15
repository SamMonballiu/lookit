using System;
using System.Drawing;

namespace Lookit.Models
{
    public class Scale
    {
        public Point First { get; set; }
        public Point Second { get; set; }
        private double _enteredDistance;
        public double EnteredDistance => _enteredDistance;
        public double ActualDistance
            => Math.Sqrt((Math.Pow(First.X - Second.X, 2) + Math.Pow(First.Y - Second.Y, 2)));
        public double Factor
            => EnteredDistance / ActualDistance;

        public ScaleUnit Unit { get; private set; }

        public Scale(Point first, Point second, double distance, ScaleUnit unit)
        {
            First = first;
            Second = second;
            _enteredDistance = distance;
            Unit = unit;
        }

        public Scale(PersistableScale scale)
        {
            First = new Point(scale.First.X, scale.First.Y);
            Second = new Point(scale.Second.X, scale.Second.Y);
            _enteredDistance = scale.EnteredDistance;
            Unit = (ScaleUnit)scale.Unit;
        }

        public void SetEnteredDistance(double distance)
        {
            _enteredDistance = distance;
        }

        public static Scale Default => new Scale(Point.Empty, Point.Empty, 0, ScaleUnit.None);

        public bool IsDefault => First.IsEmpty && Second.IsEmpty && _enteredDistance == 0;

        public Scale UpdateDistance(double distance)
        {
            return new Scale(First, Second, distance, Unit);
        }

        public override string ToString() => $"{First.X} {First.Y}, {Second.X} {Second.Y}";
    }
}
