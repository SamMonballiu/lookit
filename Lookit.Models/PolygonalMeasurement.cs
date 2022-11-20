using System.Collections.Generic;
using System.Drawing;

namespace Lookit.Models
{
    public abstract class Measurement
    {
        public List<Point> Points { get; protected set; }
    }

    public class PolygonalMeasurement : Measurement
    {
        public PolygonalMeasurement(List<Point> points)
        {
            Points = points;
        }

        public double? GetScaledArea(Scale scale)
        {
            if (scale is null)
            {
                return null;
            }

            var area = CalculateArea.CalculateArea.Polygon(Points);
            return area * scale.Factor * scale.Factor;
        }
    }
}
