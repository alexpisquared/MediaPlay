using System;
using System.Collections.Generic;

namespace DDJ.DB.Old.Models
{
    public partial class View_Last33Listened
    {
        public int ID { get; set; }
        public Nullable<System.DateTime> DateTimePlayed { get; set; }
        public string UsedBy { get; set; }
        public string PathFileName { get; set; }
    }
}
