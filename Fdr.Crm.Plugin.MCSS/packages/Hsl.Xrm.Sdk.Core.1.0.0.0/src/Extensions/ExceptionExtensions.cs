using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Hsl.Xrm.Sdk
{
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Builds a string out of the exception that can be written to the trace log.
        /// </summary>
        /// <param name="e">The exception to be transformed.</param>
        /// <returns>A human readable string containing the trace data.</returns>
        public static string ToTraceString(this Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            ex.ToTraceString(sb, string.Empty);
            return sb.ToString();
        }

        /// <summary>
        /// Recursively build the string from the exception.
        /// </summary>
        /// <param name="sb">A StringBuilder for the output string.</param>
        /// <param name="e">The Exception used to build the string.</param>
        /// <param name="indent">A string that will be prepended to every line for indentation purposes.</param>
        private static void ToTraceString(this Exception ex, StringBuilder sb, string indent)
        {
            // Blow this popsicle stand if there is not an exception
            if (ex == null) { return; }

            try
            {
                // Build the exception string
                sb.AppendLine(indent + "{" + ex.GetType().FullName + "} from {" + ex.Source + "} - " + ex.Message);
                if (ex is FaultException<OrganizationServiceFault>)
                {
                    FaultException<OrganizationServiceFault> fe = ex as FaultException<OrganizationServiceFault>;
                    if (fe != null)
                    {
                        sb.AppendFormat("\n{0}Timestamp: {1}", indent, fe.Detail.Timestamp);
                        sb.AppendFormat("\n{0}Code: {1}", indent, fe.Detail.ErrorCode);
                        sb.AppendFormat("\n{0}Message: {1}", indent, fe.Detail.Message);
                        sb.AppendFormat("\n{0}Trace: {1}", indent, fe.Detail.TraceText);
                    }
                }
                else if (ex is TimeoutException)
                {
                    TimeoutException te = ex as TimeoutException;
                    if (te != null)
                    {
                        sb.AppendFormat("\n{0}Message: {1}", indent, te.Message);
                        sb.AppendFormat("\n{0}Stack Trace: {1}", indent, te.StackTrace);
                    }
                }
                sb.AppendLine(ex.StackTrace.Replace("\n", "\n" + indent));

                // Recursively call
                ex.InnerException.ToTraceString(sb, indent + "    ");
            }
            catch
            {
                // Don't really care, just catch the exception so things keep going
            }

            return;
        }

    }
}
