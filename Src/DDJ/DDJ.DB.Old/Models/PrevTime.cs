using System;
using System.Collections.Generic;

namespace DDJ.DB.Old.Models
{
    public partial class PrevTime
    {
        public int ID { get; set; }
        public long File8bSignature { get; set; }
        public Nullable<System.DateTime> DateTimePlayed { get; set; }
        public Nullable<bool> PartyMode { get; set; }
        public string UsedBy { get; set; }
        public Nullable<System.DateTime> LastEditDate { get; set; }
        public Nullable<System.DateTime> CreationDate { get; set; }
        public virtual AudioPiece AudioPiece { get; set; }
    }
}
