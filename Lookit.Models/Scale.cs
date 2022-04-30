using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

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
        public double DistanceUnit
            => EnteredDistance / ActualDistance;

        public Scale(Point first, Point second, double distance)
        {
            First = first;
            Second = second;
            _enteredDistance = distance;
        }

        public void SetEnteredDistance(double distance)
        {
            _enteredDistance = distance;
        }

        public static Scale Default => new Scale(new Point(), new Point(), 1);
    }
}
