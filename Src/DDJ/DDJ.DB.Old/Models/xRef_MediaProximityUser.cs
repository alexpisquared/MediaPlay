using System;
using System.Collections.Generic;

namespace DDJ.DB.Old.Models
{
    public partial class xRef_MediaProximityUser
    {
        public int Id { get; set; }
        public long File8bSignature { get; set; }
        public string UserName { get; set; }
        public int ProximityStateId { get; set; }
        public Nullable<System.DateTime> LastEditDate { get; set; }
        public Nullable<System.DateTime> CreationDate { get; set; }
        public virtual AudioPiece AudioPiece { get; set; }
        public virtual lkuProximityState lkuProximityState { get; set; }
    }
}
