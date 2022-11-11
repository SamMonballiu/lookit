using System;
using System.Windows;

namespace Lookit.Logic
{
    public static class PointExtensions
    {
        public static System.Drawing.Point ToPoint(this Point point)
        {
            return new System.Drawing.Point() { X = Convert.ToInt32(point.X), Y = Convert.ToInt32(point.Y) };
        }

        public static bool IsClose(this System.Drawing.Point point, System.Drawing.Point other)
        {
            var tolerance = 5;

            return (Math.Abs(point.X - other.X) < tolerance) && (Math.Abs(point.Y - other.Y) < tolerance);
        }
    }
}
