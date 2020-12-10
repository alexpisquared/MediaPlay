using System;
using System.Collections.Generic;

namespace DDJ.DB.Old.Models
{
    public partial class AudioPieces_Tombstone
    {
        public long File8bSignature { get; set; }
        public Nullable<System.DateTime> DeletionDate { get; set; }
    }
}
