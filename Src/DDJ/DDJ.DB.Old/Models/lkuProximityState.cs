using System;
using System.Collections.Generic;

namespace DDJ.DB.Old.Models
{
    public partial class lkuProximityState
    {
        public lkuProximityState()
        {
            this.xRef_MediaProximityUser = new List<xRef_MediaProximityUser>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<xRef_MediaProximityUser> xRef_MediaProximityUser { get; set; }
    }
}
