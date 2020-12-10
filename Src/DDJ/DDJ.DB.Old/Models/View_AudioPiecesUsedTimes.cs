using System;
using System.Collections.Generic;

namespace DDJ.DB.Old.Models
{
    public partial class View_AudioPiecesUsedTimes
    {
        public long File8bSignature { get; set; }
        public string PathFileName { get; set; }
        public int nGenre { get; set; }
        public Nullable<int> Flags { get; set; }
        public Nullable<int> UsedTimes { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
    }
}
