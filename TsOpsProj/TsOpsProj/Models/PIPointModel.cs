using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TsOpsProj.Models
{
    public class PIPointModel
    {
        public string PointName { get; set; }
        public string value { get; set; }
        public string datetime { get; set; }
        public string PointClassName { get; set; }
        public string PointType { get; set; }
        public string Descriptor { get; set; }
    }
}