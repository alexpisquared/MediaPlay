using System;

namespace Common.UI.Lib.Model
{
    public class MuBookmark
    {
        public int ID { get; set; }
        public object MediaUnit { get; set; }
        public double PositionSec { get; set; }
        public double PositionMin => PositionSec / 60.0;

        //[XmlIgnore]
        public double DeltaSec { get; set; }
        public string Note { get; set; }
        public DateTime AddedAt { get; set; }
        public string AddedBy { get; set; }
    }
}
