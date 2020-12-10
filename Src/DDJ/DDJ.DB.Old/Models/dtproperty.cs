using System;
using System.Collections.Generic;

namespace DDJ.DB.Old.Models
{
    public partial class dtproperty
    {
        public int id { get; set; }
        public Nullable<int> objectid { get; set; }
        public string property { get; set; }
        public string value { get; set; }
        public string uvalue { get; set; }
        public byte[] lvalue { get; set; }
        public int version { get; set; }
    }
}
