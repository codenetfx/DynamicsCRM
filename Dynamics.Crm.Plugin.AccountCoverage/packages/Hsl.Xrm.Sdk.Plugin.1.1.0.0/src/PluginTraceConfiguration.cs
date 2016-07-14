using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Hsl.Xrm.Sdk.Plugin
{
    /// <summary>
    /// Contains the configuration data for tracing a plugin.
    /// </summary>
    public class PluginTraceConfiguration
    {
        #region Constructor & Destructor Implementations

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PluginTraceConfiguration()
        {
        }

        #endregion

        #region Property Implementations

        /// <summary>
        /// Gets/Sets an inidicates if tracing is enabled.
        /// </summary>
        public bool Enabled
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets an indication of whether the plugin context should be traced.
        /// </summary>
        public bool TraceContext
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets an indication of whether the InputParameters of the context should be traced.
        /// </summary>
        public bool TraceInputParameters
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the trace level.
        /// </summary>
        /// <remarks>
        /// This is used along with the list of <b>Users</b> to determine if entries will be written
        /// to the trace log.
        /// </remarks>
        public PluginTraceLevel TraceLevel
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets an indication of whether the OutputParameters of the context should be traced.
        /// </summary>
        public bool TraceOutputParameters
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets an indication of whether the PreEntityImages of the context should be traced.
        /// </summary>
        public bool TracePreEntityImages
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets an indication of whether the PostEntityImages of the context should be traced.
        /// </summary>
        public bool TracePostEntityImages
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets an indication of whether the SharedVariables of the context should be traced.
        /// </summary>
        public bool TraceSharedVariables
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets an indication of whether the log is traced to the event log.
        /// </summary>
        public bool TraceToEventLog
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets an indication of whether the log is traced to the tracing service.
        /// </summary>
        public bool TraceToTracingService
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets an indication of whether the log is traced to the tracing service.
        /// </summary>
        public bool TraceToAnnotation
        {
            get;
            set;
        }

        #endregion

        #region Public Method Implementations

        /// <summary>
        /// Gets the default trace configuration.
        /// </summary>
        public static PluginTraceConfiguration Default
        {
            get
            {
                PluginTraceConfiguration cfg = new PluginTraceConfiguration();

                cfg.Enabled = true;
                cfg.TraceContext = false;
                cfg.TraceInputParameters = false;
                cfg.TraceLevel = PluginTraceLevel.Exception;
                cfg.TraceOutputParameters = false;
                cfg.TracePreEntityImages = false;
                cfg.TracePostEntityImages = false;
                cfg.TraceSharedVariables = false;
                cfg.TraceToEventLog = false;
                cfg.TraceToAnnotation = false;
                cfg.TraceToTracingService = true;

                return cfg;
            }
        }

        /// <summary>
        /// Gets a trace configuration from the unsecure/secure configuration.  The secure configuration will take precendence over the unsecure
        /// configuration.  The entire trace configuration must be present in one of the configuration strings or the default configuration is returned.
        /// </summary>
        /// <param name="unsecureConfig"></param>
        /// <param name="secureConfig"></param>
        /// <returns></returns>
        public static PluginTraceConfiguration CreateFromConfiguration(string unsecureConfig, string secureConfig)
        {
            string rawConfig = null;
            XElement traceConfigXel = null;

            // Try to get the trace settings from the secure config
            rawConfig = secureConfig.TrimIfNotNull();
            traceConfigXel = (!string.IsNullOrWhiteSpace(rawConfig) && rawConfig.StartsWith("<")) ? XElement.Parse(secureConfig).Elements("TraceSettings").FirstOrDefault() : null;

            // Try to get the trace settings from the unsecure config if not in the secure config
            if (traceConfigXel == null)
            {
                rawConfig = unsecureConfig.TrimIfNotNull();
                traceConfigXel = (!string.IsNullOrWhiteSpace(rawConfig) && rawConfig.StartsWith("<")) ? XElement.Parse(unsecureConfig).Elements("TraceSettings").FirstOrDefault() : null;
            }

            // Return the default config if not specified in the configuration values
            if (traceConfigXel == null) { return PluginTraceConfiguration.Default; }

            // Parse the config from the supplied config strings
            var cfg = new PluginTraceConfiguration()
            {
                Enabled = ((bool?)traceConfigXel.Element("Enabled")).GetValueOrDefault(),
                TraceContext = ((bool?)traceConfigXel.Element("TraceContext")).GetValueOrDefault(),
                TraceInputParameters = ((bool?)traceConfigXel.Element("TraceInputParameters")).GetValueOrDefault(),
                TraceLevel = traceConfigXel.Element("TraceLevel").Value.ParseEnumOrDefault<PluginTraceLevel>(PluginTraceLevel.Exception),
                TraceOutputParameters = ((bool?)traceConfigXel.Element("TraceOutputParameters")).GetValueOrDefault(),
                TracePostEntityImages = ((bool?)traceConfigXel.Element("TracePostEntityImages")).GetValueOrDefault(),
                TracePreEntityImages = ((bool?)traceConfigXel.Element("TracePreEntityImages")).GetValueOrDefault(),
                TraceSharedVariables = ((bool?)traceConfigXel.Element("TraceSharedVariables")).GetValueOrDefault(),
                TraceToAnnotation = ((bool?)traceConfigXel.Element("TraceToAnnotation")).GetValueOrDefault(),
                TraceToEventLog = ((bool?)traceConfigXel.Element("TraceToEventLog")).GetValueOrDefault(),
                TraceToTracingService = ((bool?)traceConfigXel.Element("TraceToTracingService")).GetValueOrDefault(true)
            };

            return cfg;
        }

        #endregion
    }
}
