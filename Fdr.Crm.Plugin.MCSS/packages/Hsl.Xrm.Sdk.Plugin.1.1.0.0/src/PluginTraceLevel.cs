using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hsl.Xrm.Sdk.Plugin
{
    /// <summary>
    /// Indicates the level of tracing that will be captured by a plugin.
    /// </summary>
    public enum PluginTraceLevel
    {
        None = 0,
        Exception,
        Error,
        Warning,
        Info,
        Verbose,
    }
}
