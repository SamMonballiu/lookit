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

        public static System.Drawing.Point Align(this System.Drawing.Point point, System.Drawing.Point other, int tolerance)
        {
            var horizontalDifference = Math.Abs(point.X - other.X);
            var verticalDifference = Math.Abs(point.Y - other.Y);

            if (horizontalDifference <= tolerance)
            {
                return new System.Drawing.Point(other.X, point.Y);
            }

            if (verticalDifference <= tolerance)
            {
                return new System.Drawing.Point(point.X, other.Y);
            }

            return point;
        }
    }
}
