using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.OData
{
    public class OpsCity : BaseOpsCity
    {
        public Int32 Id { get; set; }
    }

    public abstract class BaseOpsCity
    {
        public String Name { get; set; }
    }
}
