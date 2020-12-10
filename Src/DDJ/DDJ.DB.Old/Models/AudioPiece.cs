using System;
using System.Collections.Generic;

namespace DDJ.DB.Old.Models
{
    public partial class AudioPiece
    {
        public AudioPiece()
        {
            this.LikedHateds = new List<LikedHated>();
            this.PrevTimes = new List<PrevTime>();
            this.xRef_MediaProximityUser = new List<xRef_MediaProximityUser>();
        }

        public long File8bSignature { get; set; }
        public string PathFileName { get; set; }
        public Nullable<int> FileLengh { get; set; }
        public Nullable<int> Flags { get; set; }
        public int nGenre { get; set; }
        public Nullable<int> Genre { get; set; }
        public string tagTitle { get; set; }
        public string tagArtist { get; set; }
        public string tagAlbum { get; set; }
        public string tagComment { get; set; }
        public string tagGenre { get; set; }
        public Nullable<System.DateTime> AddedAt { get; set; }
        public string AddedBy { get; set; }
        public string Nootes { get; set; }
        public Nullable<System.DateTime> LastEditDate { get; set; }
        public Nullable<System.DateTime> CreationDate { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public virtual lkuGenre lkuGenre { get; set; }
        public virtual ICollection<LikedHated> LikedHateds { get; set; }
        public virtual ICollection<PrevTime> PrevTimes { get; set; }
        public virtual ICollection<xRef_MediaProximityUser> xRef_MediaProximityUser { get; set; }
    }
}
