using Microsoft.AspNet.OData.Query;

namespace AutoMapper.AspNet.OData
{
    /// <summary>
    /// This class describes the settings to use during query composition.
    /// </summary>
    public class QuerySettings
    {
        /// <summary>
        /// Gets or sets a value indicating how null propagation should
        /// be handled during query composition.
        /// </summary>
        /// <value>
        /// The default is <see cref="F:Microsoft.AspNet.OData.Query.HandleNullPropagationOption.Default" />.
        /// </value>
        public HandleNullPropagationOption HandleNullPropagation { get; set; } = HandleNullPropagationOption.Default;

        /// <summary>
        /// Miscellaneous arguments for IMapper.ProjectTo
        /// </summary>
        public ProjectionSettings ProjectionSettings { get; set; }

    }
}