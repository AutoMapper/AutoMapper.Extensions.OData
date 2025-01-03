using System;
using System.Collections.Generic;

namespace AutoMapper.AspNet.OData.Expansions
{
    abstract public class Expansion
    {
        public string MemberName { get; set; }
        public Type MemberType { get; set; }
        public Type ParentType { get; set; }
        public List<string> Selects { get; set; }
    }
}
