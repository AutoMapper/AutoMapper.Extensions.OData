using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.OData
{
    public class OpsTenant
    {
        [Key]
        public Guid Identity { get; set; }
        public string Name { get; set; }
        public ICollection<CoreBuilding> Buildings { get; set; }
    }
}
