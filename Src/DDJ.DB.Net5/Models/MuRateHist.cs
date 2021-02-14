namespace DDJ.DB.Models
{
    public partial class MuRateHist
    {
        public int ID { get; set; }
        public int MediaUnitID { get; set; }
        public int LikeUnit { get; set; }
        public System.DateTime DoneAt { get; set; }
        public string DoneBy { get; set; }
				public double CurPositionSec { get; set; }
				public virtual MediaUnit MediaUnit { get; set; }
    }
}
