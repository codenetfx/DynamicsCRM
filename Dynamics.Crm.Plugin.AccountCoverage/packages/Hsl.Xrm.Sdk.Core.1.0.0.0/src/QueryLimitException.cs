using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Hsl.Xrm.Sdk
{
    [Serializable]
    public class QueryLimitException : Exception
    {
        public QueryLimitException()
        {
        }

        public QueryLimitException(string message)
            : base(message)
        {
        }

        public QueryLimitException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected QueryLimitException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}
