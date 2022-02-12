using Microsoft.AspNetCore.OData.Query;
using System;


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

        /// <summary>
        /// Gets or sets the maximum number of query results to return.
        /// </summary>
        /// <value>
        /// The maximum number of query results to return, or null if there is no limit. Default is null.
        /// </value>
        public int? PageSize { get; set; }

        /// <summary>
        /// Gets of sets the <see cref="TimeZoneInfo"/>.
        /// </summary>
        /// <value>
        /// Default is null.
        /// </value>
        public TimeZoneInfo TimeZone { get; set; }
    }
}
