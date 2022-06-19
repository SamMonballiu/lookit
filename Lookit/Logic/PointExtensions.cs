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
    }
}
