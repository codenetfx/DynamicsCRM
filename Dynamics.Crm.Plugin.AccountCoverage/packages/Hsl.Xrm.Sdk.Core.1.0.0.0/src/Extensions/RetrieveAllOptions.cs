using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hsl.Xrm.Sdk
{
    public class RetrieveAllOptions
    {
        public const int DefaultMaxResults = 50000;
        public RetrieveAllOptions()
            : this(true, DefaultMaxResults)
        {
        }

        public RetrieveAllOptions(bool errorAtMaxResults, int maxResults, int pageSize = 0)
        {
            ErrorAtMaxResults = errorAtMaxResults;
            PageSize = pageSize;
            MaxResults = maxResults;
        }

        /// <summary>
        /// Determines whether an error is thrown when max results is reached
        /// </summary>
        public bool ErrorAtMaxResults { get; set; }

        /// <summary>
        /// The maximum number of results to return. Default: 50k
        /// </summary>
        public int MaxResults { get; set; }

        /// <summary>
        /// Number of results per request batch from the server.
        /// </summary>
        public int PageSize { get; set; }
    }
}
