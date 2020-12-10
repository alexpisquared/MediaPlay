using System;
using System.Collections.Generic;

namespace DDJ.DB.Old.Models
{
    public partial class LikedHated
    {
        public int ID { get; set; }
        public long File8bSignature { get; set; }
        public Nullable<int> IsGood { get; set; }
        public Nullable<System.DateTime> EvaluatedAt { get; set; }
        public string EvaluatedBy { get; set; }
        public Nullable<System.DateTime> LikedAt { get; set; }
        public Nullable<System.DateTime> HatedAt { get; set; }
        public Nullable<System.DateTime> LastEditDate { get; set; }
        public Nullable<System.DateTime> CreationDate { get; set; }
        public virtual AudioPiece AudioPiece { get; set; }
    }
}
