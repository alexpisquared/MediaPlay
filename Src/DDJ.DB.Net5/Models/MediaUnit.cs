using System;
using System.Collections.Generic;

namespace DDJ.DB.Models
{
    public partial class MediaUnit
    {
        public MediaUnit()
        {
            MuAuditions = new List<MuAudition>();
            MuBookmarks = new List<MuBookmark>();
            MuRateHists = new List<MuRateHist>();
        }

        public int ID { get; set; }
        public int GenreID { get; set; }
        public string PathFileExtOrg { get; set; }
        public string PathName { get; set; }
        public string FileName { get; set; }
        public long FileHashMD5 { get; set; }
        public long FileHashQck { get; set; }
        public long FileLength { get; set; }
				//public double DurationSec { get; set; }
				//public double CurPositionSec { get; set; }
        public string Notes { get; set; }
        public System.DateTime AddedAt { get; set; }
        public Nullable<System.DateTime> DeletedAt { get; set; }
        public virtual LkuGenre LkuGenre { get; set; }
        public virtual ICollection<MuAudition> MuAuditions { get; set; }
        public virtual ICollection<MuBookmark> MuBookmarks { get; set; }
        public virtual ICollection<MuRateHist> MuRateHists { get; set; }
    }
}
