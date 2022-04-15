using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.OData
{
    public class OpsTenant : BaseOpsTenant
    {
        [Key]
        public Guid Identity { get; set; }
        public DateTime CreatedDate { get; set; }
        public ICollection<CoreBuilding> Buildings { get; set; }
    }

    public abstract class BaseOpsTenant
    {
        public string Name { get; set; }
    }
}
