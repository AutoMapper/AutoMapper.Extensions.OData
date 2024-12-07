using Microsoft.OData.UriParser;

namespace AutoMapper.AspNet.OData
{
    internal class OrderBySetting
    {
        public string Name { get; set; }
        public OrderByDirection Direction { get; set; } = OrderByDirection.Ascending;
        public OrderBySetting ThenBy { get; set; }
    }
}