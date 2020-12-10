using System;
using System.Collections.Generic;

namespace DDJ.DB.Old.Models
{
    public partial class View_UsageSortedUndeleted
    {
        public long File8bSignature { get; set; }
        public string PathFileName { get; set; }
        public int nGenre { get; set; }
        public Nullable<int> TimesPlayed { get; set; }
        public System.DateTime LastTimePlayed { get; set; }
        public Nullable<int> Liked { get; set; }
        public Nullable<int> Hated { get; set; }
    }
}
