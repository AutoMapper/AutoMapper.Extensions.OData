using Microsoft.AspNet.OData.Query;

namespace AutoMapper.AspNet.OData
{
    /// <summary>
    /// Settings for configuring OData options on the server
    /// </summary>
    public class ODataSettings
    {
        /// <summary>
        /// Gets or sets a value indicating how null propagation should
        /// be handled during query composition.
        /// </summary>
        /// <value>
        /// The default is <see cref="F:Microsoft.AspNet.OData.Query.HandleNullPropagationOption.Default" />.
        /// </value>
        public HandleNullPropagationOption HandleNullPropagation { get; set; } = HandleNullPropagationOption.Default;
    }
}
