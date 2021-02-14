using System.Collections.Generic;

namespace DDJ.DB.Models
{
    public partial class LkuGenre
    {
        public LkuGenre()
        {
            MediaUnits = new List<MediaUnit>();
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public virtual ICollection<MediaUnit> MediaUnits { get; set; }
    }
}
