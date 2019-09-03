using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.OData
{
    public class CoreBuilding
    {
        [Key]
        public Guid Identity { get; set; }
        public string Name { get; set; }
        public OpsBuilder Builder { get; set; }
        public OpsTenant Tenant { get; set; }
    }
}
