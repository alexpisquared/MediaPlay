using System;

namespace DDJ.DB.Models
{
    public partial class View_AudioPiecesUsedTimes
    {
        public int MediaUnitID { get; set; }
        public string PathFileName { get; set; }
        public int GenreID { get; set; }
        public string FileName { get; set; }
        public Nullable<int> UsedTimesCount { get; set; }
    }
}
