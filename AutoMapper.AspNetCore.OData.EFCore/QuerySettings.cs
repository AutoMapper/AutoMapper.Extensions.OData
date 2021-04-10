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

        /// <summary>
        /// Async Settings hold the cancellation token for async requests
        /// </summary>
        public AsyncSettings AsyncSettings { get; set; }
    }
}