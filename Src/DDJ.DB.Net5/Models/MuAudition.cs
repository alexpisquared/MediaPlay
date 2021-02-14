namespace DDJ.DB.Models
{
    public partial class MuAudition
    {
        public int ID { get; set; }
        public int MediaUnitID { get; set; }
        public bool PartyMode { get; set; }
        public System.DateTime DoneAt { get; set; }
        public string DoneBy { get; set; }
        public virtual MediaUnit MediaUnit { get; set; }
    }
}
