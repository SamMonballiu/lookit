using System.Collections.Generic;

namespace Lookit.Models
{
    public class PersistableSession
    {
        public int SelectedPage { get; set; }
        public string PdfPath { get; set; }
        public Dictionary<int, PersistableScale> PagedScales { get; set; }
        public Dictionary<int, List<PersistableMeasurement>> PagedMeasurements { get; set; }
    }

    public class PersistableMeasurement
    {
        public string Name { get; set; }
        public IList<PersistablePoint> Points { get; set; }
    }

    public class PersistableScale
    {
        public PersistablePoint First { get; set; }
        public PersistablePoint Second { get; set; }
        public double EnteredDistance { get; set; }
        public int Unit { get; set; }
    }

    public class PersistablePoint
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}
