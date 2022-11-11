using System.Collections.Generic;
using System.Drawing;

namespace Lookit.Models.CalculateArea
{
    internal static class CalculateArea
    {
        // https://www.codeproject.com/Tips/601272/Calculating-the-area-of-a-polygon
        public static double Polygon(IEnumerable<Point> polygon)
        {
            var e = polygon.GetEnumerator();
            if (!e.MoveNext()) return 0;
            Point first = e.Current, last = first;

            double area = 0;
            while (e.MoveNext())
            {
                Point next = e.Current;
                area += next.X * last.Y - last.X * next.Y;
                last = next;
            }
            area += first.X * last.Y - last.X * first.Y;
            return area / 2;
        }
    }
}
