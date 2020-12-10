using System;
using System.Collections.Generic;

namespace DDJ.DB.Old.Models
{
    public partial class FileSysFile
    {
        public int ID { get; set; }
        public string PathFileName { get; set; }
        public System.DateTime AddetAt { get; set; }
        public Nullable<long> Crc { get; set; }
        public Nullable<System.DateTime> LastEditDate { get; set; }
        public Nullable<System.DateTime> CreationDate { get; set; }
    }
}
