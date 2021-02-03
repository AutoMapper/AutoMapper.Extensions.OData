using Microsoft.AspNet.OData.Query;

namespace AutoMapper.AspNet.OData
{
    /// <summary>
    /// This class describes the settings to use during query composition.
    /// </summary>
    public class QuerySettings
    {
        /// <summary>
        /// Settings for configuring OData options on the server
        /// </summary>
        public ODataSettings ODataSettings { get; set; }

        /// <summary>
        /// Miscellaneous arguments for IMapper.ProjectTo
        /// </summary>
        public ProjectionSettings ProjectionSettings { get; set; }

    }
}