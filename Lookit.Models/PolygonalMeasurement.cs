using System.Collections.Generic;
using System.Drawing;

namespace Lookit.Models
{
    public class PolygonalMeasurement
    {
        public List<Point> Points { get; private set; }

        public PolygonalMeasurement(List<Point> points)
        {
            Points = points;
        }

        public double Area => CalculateArea.CalculateArea.Polygon(Points);

        public double? GetScaledArea(Scale scale)
        {
            if (scale is null)
            {
                return null;
            }

            return Area * scale.Factor * scale.Factor;
        }
    }
}
