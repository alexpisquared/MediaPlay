namespace DDJ.DB.Models
{
    public partial class MuBookmark
    {
        public int ID { get; set; }
        public int MediaUnitID { get; set; }
        public double PositionSec { get; set; }
        public string Note { get; set; }
        public System.DateTime DoneAt { get; set; }
        public string DoneBy { get; set; }
        public virtual MediaUnit MediaUnit { get; set; }
    }
}
