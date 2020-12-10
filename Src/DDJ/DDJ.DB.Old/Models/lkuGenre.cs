using System;
using System.Collections.Generic;

namespace DDJ.DB.Old.Models
{
    public partial class lkuGenre
    {
        public lkuGenre()
        {
            this.AudioPieces = new List<AudioPiece>();
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public virtual ICollection<AudioPiece> AudioPieces { get; set; }
    }
}
