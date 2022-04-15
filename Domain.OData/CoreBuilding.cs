using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.OData
{
    public class CoreBuilding : BaseCoreBuilding
    {
        [Key]
        public Guid Identity { get; set; }
        public OpsBuilder Builder { get; set; }
        public OpsTenant Tenant { get; set; }
        public string Parameter { get; set; }
    }

    public abstract class BaseCoreBuilding
    {
        public string Name { get; set; }
    }
}
