using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.OData
{
    public class OpsBuilder
    {
        public Int32 Id { get; set; }
        public String Name { get; set; }
        public OpsCity City { get; set; }
        public Int32 Parameter { get; set; }
    }
}
