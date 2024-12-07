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

        /// <summary>
        /// Gets or sets a value indicating whether constants should be parameterized.
        /// </summary>
        /// <value>
        /// Default is true.
        /// </value>
        public bool EnableConstantParameterization { get; set; } = true;
        
        /// <summary>
        /// If sets to true, orderBy pk desc will always be present on main entity. 
        /// </summary>
        /// <example>
        /// SELECT *
        /// FROM "TEntitiy" AS "c"
        /// ORDER BY "c"."Id" DESC
        /// In case some orderBy was passed, additional thenBy pk will be applied
        /// SELECT *
        /// FROM "TEntitiy" AS "c"
        /// ORDER BY "c"."Type" DESC, "c"."Id" DESC
        /// </example>
        /// <value>
        /// Default is false.
        /// </value>
        public bool AlwaysSortByPrimaryKey { get; set; }
    }
}
