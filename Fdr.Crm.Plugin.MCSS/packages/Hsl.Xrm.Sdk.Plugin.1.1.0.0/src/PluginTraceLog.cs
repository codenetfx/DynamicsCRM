using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;

namespace Hsl.Xrm.Sdk.Plugin
{
    public class PluginTraceLog
    {
        private const string CrmEventLogSource = "MSCRMWebService";

        #region Constructor & Destructor Implementations

        /// <summary>
        /// Default constructor.
        /// </summary>
        private PluginTraceLog()
        {
        }

        #endregion

        #region Property Implementations

        public StringBuilder Buffer { get; private set; }

        public IPluginExecutionContext Context
        {
            get
            {
                if (_Context == null)
                {
                    if (this.ServiceProvider == null)
                    {
                        throw new InvalidOperationException("An IPluginExecutionContext cannot be accessed until a valid IServiceProvider exists.");
                    }

                    _Context = this.ServiceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
                    if (_Context == null)
                    {
                        throw new InvalidOperationException("An IPluginExecutionContext could not be retrieved using the IServiceProvider.");
                    }
                }

                return _Context;
            }
        }
        private IPluginExecutionContext _Context;

        public IOrganizationService OrgService
        {
            get
            {
                if (_OrgService == null)
                {
                    if (this.ServiceProvider == null)
                    {
                        throw new InvalidOperationException("An IOrganizationService cannot be accessed until a valid IServiceProvider exists.");
                    }

                    var serviceFactory = this.ServiceProvider.GetService(typeof(IOrganizationServiceFactory)) as IOrganizationServiceFactory;
                    _OrgService = (serviceFactory != null) ? serviceFactory.CreateOrganizationService(null) : null;

                    if (_OrgService == null)
                    {
                        throw new InvalidOperationException("An IOrganizationService could not be retrieved using the IOrganizationServiceFactory retrieved using the IServiceProvider.");
                    }
                }

                return _OrgService;
            }
        }
        private IOrganizationService _OrgService;

        public string PluginName { get; private set; }

        public IServiceProvider ServiceProvider { get; private set; }

        public PluginTraceConfiguration Config { get; private set; }

        public ITracingService TracingService
        {
            get
            {
                if (_TracingService == null)
                {
                    if (this.ServiceProvider == null)
                    {
                        throw new InvalidOperationException("An ITracingService cannot be accessed until a valid IServiceProvider exists.");
                    }

                    _TracingService = this.ServiceProvider.GetService(typeof(ITracingService)) as ITracingService;
                    if (_Context == null)
                    {
                        throw new InvalidOperationException("An ITracingService could not be retrieved using the IServiceProvider.");
                    }
                }

                return _TracingService;
            }
        }
        private ITracingService _TracingService;

        #endregion

        #region Public Method Implementations

        public static PluginTraceLog Create(string pluginName, PluginTraceConfiguration config, IServiceProvider serviceProvider)
        {
            var log = new PluginTraceLog();
            log.PluginName = pluginName;
            log.Config = config;
            log.Buffer = new StringBuilder();
            log.ServiceProvider = serviceProvider;

            return log;
        }

        /// <summary>
        /// Saves the trace data to disk.
        /// </summary>
        public void Save()
        {
            if (this.Buffer.Length > 0)
            {
                // Save the log to the various media
                WriteBufferToTraceService();
                WriteBufferToNote();
                WriteBufferToEventLog();
            }
        }

        #endregion

        #region Write & Trace Method Implementations

        /// <summary>
        /// Traces the data in the plugin execution context to the log.
        /// </summary>
        public void TraceContext()
        {
            // Trace if the configured to do so
            if (TraceShouldBeWritten() && this.Config.TraceContext)
            {
                StringBuilder sb = new StringBuilder();
                PluginTraceLog.AppendHeader(sb, PluginTraceLevel.Verbose);
                PluginTraceLog.TransformContextToTraceString(sb, this.Context, string.Empty);
                PluginTraceLog.AppendFooter(sb);
                WriteToBuffer(sb.ToString());
            }
        }

        /// <summary>
        /// Traces the data in the plugin execution context input parameters to the log.
        /// </summary>
        public void TraceContextInputParameters()
        {
            // Trace if the configured to do so
            if (TraceShouldBeWritten() && this.Config.TraceInputParameters)
            {
                StringBuilder sb = new StringBuilder();
                PluginTraceLog.AppendHeader(sb, PluginTraceLevel.Verbose);
                sb.AppendLine("*** Plugin Execution Context - InputParameters ***");
                PluginTraceLog.TransformParameterCollectionToTraceString(sb, this.Context.InputParameters, string.Empty);
                PluginTraceLog.AppendFooter(sb);
                WriteToBuffer(sb.ToString());
            }
        }

        /// <summary>
        /// Traces the data in the plugin execution context input parameters to the log.
        /// </summary>
        public void TraceContextOutputParameters()
        {
            // Trace if the configured to do so
            if (TraceShouldBeWritten() && this.Config.TraceOutputParameters)
            {
                StringBuilder sb = new StringBuilder();
                PluginTraceLog.AppendHeader(sb, PluginTraceLevel.Verbose);
                sb.AppendLine("*** Plugin Execution Context - OutputParameters ***");
                PluginTraceLog.TransformParameterCollectionToTraceString(sb, this.Context.OutputParameters, string.Empty);
                PluginTraceLog.AppendFooter(sb);
                WriteToBuffer(sb.ToString());
            }
        }

        /// <summary>
        /// Traces the data in the plugin execution context input parameters to the log.
        /// </summary>
        public void TraceContextPreEntityImages()
        {
            // Trace if the configured to do so
            if (TraceShouldBeWritten() && this.Config.TracePreEntityImages)
            {
                StringBuilder sb = new StringBuilder();
                PluginTraceLog.AppendHeader(sb, PluginTraceLevel.Verbose);
                sb.AppendLine("*** Plugin Execution Context - PreEntityImages ***");
                PluginTraceLog.TransformEntityImageCollectionToTraceString(sb, this.Context.PreEntityImages, string.Empty);
                PluginTraceLog.AppendFooter(sb);
                WriteToBuffer(sb.ToString());
            }
        }

        /// <summary>
        /// Traces the data in the plugin execution context input parameters to the log.
        /// </summary>
        public void TraceContextPostEntityImages()
        {
            // Trace if the configured to do so
            if (TraceShouldBeWritten() && this.Config.TracePostEntityImages)
            {
                StringBuilder sb = new StringBuilder();
                PluginTraceLog.AppendHeader(sb, PluginTraceLevel.Verbose);
                sb.AppendLine("*** Plugin Execution Context - PostEntityImages ***");
                PluginTraceLog.TransformEntityImageCollectionToTraceString(sb, this.Context.PostEntityImages, string.Empty);
                PluginTraceLog.AppendFooter(sb);
                WriteToBuffer(sb.ToString());
            }
        }

        /// <summary>
        /// Traces the data in the plugin execution context input parameters to the log.
        /// </summary>
        public void TraceContextSharedVariables()
        {
            // Trace if the configured to do so
            if (TraceShouldBeWritten() && this.Config.TraceSharedVariables)
            {
                StringBuilder sb = new StringBuilder();
                PluginTraceLog.AppendHeader(sb, PluginTraceLevel.Verbose);
                sb.AppendLine("*** Plugin Execution Context - SharedVariables ***");

                var ctx = this.Context;
                var indent = string.Empty;
                do
                {
                    sb.AppendFormat("{0}*** SharedVariable Context [MessageName = {1}, PrimaryEntityName = {2}, PrimaryEntityId = {3:B}, Stage = {4}, Depth = {5}] ***\n", indent, ctx.MessageName, ctx.PrimaryEntityName, ctx.PrimaryEntityId, ctx.Stage, ctx.Depth);
                    PluginTraceLog.TransformParameterCollectionToTraceString(sb, ctx.SharedVariables, indent);
                    sb.AppendLine();
                    ctx = ctx.ParentContext;
                } while (ctx != null);

                //PluginTraceLog.TransformParameterCollectionToTraceString(sb, this.Context.SharedVariables, string.Empty);
                PluginTraceLog.AppendFooter(sb);
                WriteToBuffer(sb.ToString());
            }
        }

        /// <summary>
        /// Traces the supplied entity and a custom message.
        /// </summary>
        public void TraceEntity(Entity e, string msg, params object[] args)
        {
            StringBuilder sb = new StringBuilder();
            PluginTraceLog.AppendHeader(sb, PluginTraceLevel.Verbose);
            sb.AppendLine(FormatIfParams(msg, args));
            PluginTraceLog.TransformEntityToTraceString(sb, e, string.Empty);
            PluginTraceLog.AppendFooter(sb);
            WriteToBuffer(sb.ToString());
        }

        /// <summary>
        /// Traces an exception to the log by writing the data associated with the exception.
        /// </summary>
        /// <param name="msg">A descriptive message to be written to the log.</param>
        /// <param name="ex">The exception to be traced.</param>
        /// <param name="args">String format args to apply to the message</param>
        public void TraceException(Exception ex, string msg, params object[] args)
        {
            // Determine if the trace should to be written
            if (TraceShouldBeWritten(PluginTraceLevel.Exception))
            {
                // Write the trace data
                StringBuilder sb = new StringBuilder();
                PluginTraceLog.AppendHeader(sb, PluginTraceLevel.Exception);

                if (msg != null)
                {
                    sb.AppendLine(FormatIfParams(msg, args));
                }
                sb.AppendLine();
                sb.AppendLine(TransformExceptionToTraceString(ex));
                PluginTraceLog.AppendFooter(sb);

                WriteToBuffer(sb.ToString());
            }
        }

        /// <summary>
        /// Traces the exception to the log if the condition is true.
        /// </summary>
        /// <param name="condition">The condition evaluated before writing to the trace log.</param>
        /// <param name="msg">The message to be written to the log if the supplied condition is true.</param>
        /// <param name="ex"></param>
        /// <param name="args">String format args to apply to the message</param>
        public void TraceExceptionIf(bool condition, Exception ex, string msg, params object[] args)
        {
            if (condition)
            {
                TraceException(ex, FormatIfParams(msg, args));
            }
        }

        /// <summary>
        /// Traces an exception to the log by writing the data associated with the exception.
        /// </summary>
        /// <param name="ex">The exception to be traced.</param>
        /// <param name="msgFunc">A function that will return an additional message to be traced.</param>
        public void TraceException(Exception ex, Func<string> msgFunc)
        {
            if (TraceShouldBeWritten(PluginTraceLevel.Exception))
            {
                var msgFn = msgFunc ?? GetNullString;
                TraceException(ex, msgFn());
            }
        }

        /// <summary>
        /// Traces the exception to the log if the condition is true.
        /// </summary>
        /// <param name="condition">The condition evaluated before writing to the trace log.</param>
        /// <param name="ex">The exception to be traced.</param>
        /// <param name="msgFunc">A function that will return an additional message to be traced.</param>
        public void TraceExceptionIf(bool condition, Exception ex, Func<string> msgFunc)
        {
            if (condition)
            {
                TraceException(ex, msgFunc);
            }
        }

        /// <summary>
        /// Traces the error to the log.
        /// </summary>
        /// <param name="msg">The message to be written to the log.</param>
        /// <param name="args">String format args to apply to the message</param>
        public void TraceError(string msg, params object[] args)
        {
            // Determine if the trace should to be written
            if (TraceShouldBeWritten(PluginTraceLevel.Error))
            {
                // Write the trace data
                StringBuilder sb = new StringBuilder();
                PluginTraceLog.AppendHeader(sb, PluginTraceLevel.Error);
                if (msg != null)
                {
                    sb.AppendLine(FormatIfParams(msg, args));
                }
                PluginTraceLog.AppendFooter(sb);

                WriteToBuffer(sb.ToString());
            }
        }

        /// <summary>
        /// Traces the error to the log if the condition is true.
        /// </summary>
        /// <param name="condition">The condition evaluated before writing to the trace log.</param>
        /// <param name="msg">The message to be written to the log if the supplied condition is true.</param>
        /// <param name="args">String format args to apply to the message</param>
        public void TraceErrorIf(bool condition, string msg, params object[] args)
        {
            if (condition)
            {
                TraceError(FormatIfParams(msg, args));
            }
        }

        /// <summary>
        /// Traces an error message to the trace log..
        /// </summary>
        /// <param name="msgFunc">A function that will return an additional message to be traced.</param>
        public void TraceError(Func<string> msgFunc)
        {
            if (TraceShouldBeWritten(PluginTraceLevel.Error))
            {
                var msgFn = msgFunc ?? GetNullString;
                TraceError(msgFn());
            }
        }

        /// <summary>
        /// Traces the error to the log if the condition is true.
        /// </summary>
        /// <param name="condition">The condition evaluated before writing to the trace log.</param>
        /// <param name="msgFunc">A function that will return an additional message to be traced.</param>
        public void TraceErrorIf(bool condition, Func<string> msgFunc)
        {
            if (condition)
            {
                TraceError(msgFunc);
            }
        }

        /// <summary>
        /// Traces the warning to the log.
        /// </summary>
        /// <param name="msg">The message to be written to the log.</param>
        /// <param name="args">String format args to apply to the message</param>
        public void TraceWarning(string msg, params object[] args)
        {
            // Determine if the trace should to be written
            if (TraceShouldBeWritten(PluginTraceLevel.Warning))
            {
                // Write the trace data
                StringBuilder sb = new StringBuilder();
                PluginTraceLog.AppendHeader(sb, PluginTraceLevel.Warning);

                if (msg != null)
                {
                    sb.AppendLine(FormatIfParams(msg, args));
                }
                PluginTraceLog.AppendFooter(sb);

                WriteToBuffer(sb.ToString());
            }
        }

        /// <summary>
        /// Traces the warning to the log if the condition is true.
        /// </summary>
        /// <param name="condition">The condition evaluated before writing to the trace log.</param>
        /// <param name="msg">The message to be written to the log if the supplied condition is true.</param>
        /// <param name="args">String format args to apply to the message</param>
        public void TraceWarningIf(bool condition, string msg, params object[] args)
        {
            if (condition)
            {
                TraceWarning(FormatIfParams(msg, args));
            }
        }

        /// <summary>
        /// Traces the warning to the log.
        /// </summary>
        /// <param name="msgFunc">A function that will return an additional message to be traced.</param>
        public void TraceWarning(Func<string> msgFunc)
        {
            if (TraceShouldBeWritten(PluginTraceLevel.Warning))
            {
                var msgFn = msgFunc ?? GetNullString;
                TraceWarning(msgFn());
            }
        }

        /// <summary>
        /// Traces the warning to the log if the condition is true.
        /// </summary>
        /// <param name="condition">The condition evaluated before writing to the trace log.</param>
        /// <param name="msgFunc">A function that will return an additional message to be traced.</param>
        public void TraceWarningIf(bool condition, Func<string> msgFunc)
        {
            if (condition)
            {
                TraceWarning(msgFunc);
            }
        }

        /// <summary>
        /// Traces the information to the log.
        /// </summary>
        /// <param name="msg">The message to be written to the log.</param>
        /// <param name="args">String format args to apply to the message</param>
        public void TraceInfo(string msg, params object[] args)
        {
            // Determine if the trace should to be written
            if (TraceShouldBeWritten(PluginTraceLevel.Info))
            {
                // Write the trace data
                StringBuilder sb = new StringBuilder();
                PluginTraceLog.AppendHeader(sb, PluginTraceLevel.Info);

                if (msg != null)
                {
                    sb.AppendLine(FormatIfParams(msg, args));
                }
                PluginTraceLog.AppendFooter(sb);

                WriteToBuffer(sb.ToString());
            }
        }

        /// <summary>
        /// Traces the information to the log if the condition is true.
        /// </summary>
        /// <param name="condition">The condition evaluated before writing to the trace log.</param>
        /// <param name="msg">The message to be written to the log if the supplied condition is true.</param>
        /// <param name="args">String format args to apply to the message</param>
        public void TraceInfoIf(bool condition, string msg, params object[] args)
        {
            if (condition)
            {
                TraceInfo(FormatIfParams(msg, args));
            }
        }

        /// <summary>
        /// Traces the information to the log.
        /// </summary>
        /// <param name="msgFunc">A function that will return an additional message to be traced.</param>
        public void TraceInfo(Func<string> msgFunc)
        {
            if (TraceShouldBeWritten(PluginTraceLevel.Info))
            {
                var msgFn = msgFunc ?? GetNullString;
                TraceInfo(msgFn());
            }
        }

        /// <summary>
        /// Traces the information to the log if the condition is true.
        /// </summary>
        /// <param name="condition">The condition evaluated before writing to the trace log.</param>
        /// <param name="msgFunc">A function that will return an additional message to be traced.</param>
        public void TraceInfoIf(bool condition, Func<string> msgFunc)
        {
            if (condition)
            {
                TraceInfo(msgFunc);
            }
        }

        /// <summary>
        /// Traces the verbosity to the log.
        /// </summary>
        /// <param name="msg">The message to be written to the log.</param>
        /// <param name="args">String format args to apply to the message</param>
        public void TraceVerbose(string msg, params object[] args)
        {
            // Determine if the trace should to be written
            if (TraceShouldBeWritten(PluginTraceLevel.Verbose))
            {
                // Write the trace data
                StringBuilder sb = new StringBuilder();

                PluginTraceLog.AppendHeader(sb, PluginTraceLevel.Verbose);
                if (msg != null)
                {
                    sb.AppendLine(FormatIfParams(msg, args));
                }
                PluginTraceLog.AppendFooter(sb);

                WriteToBuffer(sb.ToString());
            }
        }

        /// <summary>
        /// Traces the verbosity to the log if the condition is true.
        /// </summary>
        /// <param name="condition">The condition evaluated before writing to the trace log.</param>
        /// <param name="msg">The message to be written to the log if the supplied condition is true.</param>
        /// <param name="args">String format args to apply to the message</param>
        public void TraceVerboseIf(bool condition, string msg, params object[] args)
        {
            if (condition)
            {
                TraceVerbose(FormatIfParams(msg, args));
            }
        }

        /// <summary>
        /// Traces the verbosity to the log.
        /// </summary>
        /// <param name="msgFunc">A function that will return an additional message to be traced.</param>
        public void TraceVerbose(Func<string> msgFunc)
        {
            if (TraceShouldBeWritten(PluginTraceLevel.Info))
            {
                var msgFn = msgFunc ?? GetNullString;
                TraceVerbose(msgFn());
            }
        }

        /// <summary>
        /// Traces the verbosity to the log if the condition is true.
        /// </summary>
        /// <param name="condition">The condition evaluated before writing to the trace log.</param>
        /// <param name="msgFunc">A function that will return an additional message to be traced.</param>
        public void TraceVerboseIf(bool condition, Func<string> msgFunc)
        {
            if (condition)
            {
                TraceVerbose(msgFunc);
            }
        }

        /// <summary>
        /// Builds a string out of the exception that can be written to the trace log.
        /// </summary>
        /// <param name="e">The exception to be transformed.</param>
        /// <returns>A human readable string containing the trace data.</returns>
        public static string TransformExceptionToTraceString(Exception e)
        {
            StringBuilder sb = new StringBuilder();
            PluginTraceLog.TransformExceptionToTraceString(sb, e, string.Empty);
            return sb.ToString();
        }

        /// <summary>
        /// Write the message to the trace log.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="msg">The message to be written to the trace log.</param>
        /// <param name="args">String format args to apply to the message</param>
        public void Write(PluginTraceLevel level, string msg, params object[] args)
        {
            // Make sure tracing is active
            if (TraceShouldBeWritten(level))
            {
                WriteToBuffer(FormatIfParams(msg, args));
            }
        }

        /// <summary>
        /// Writes the message followed by a newline to the trace log if the log is configured to trace
        /// data of the specified level.
        /// </summary>
        /// <param name="level">The trace level of the data.</param>
        /// <param name="msg">The message to be traced.</param>
        /// <param name="args">String format args to apply to the message</param>
        public void WriteLine(PluginTraceLevel level, string msg, params object[] args)
        {
            Write(level, FormatIfParams(msg, args) + Environment.NewLine);
        }

        #endregion

        #region Private Method Implementations

        private static string GetNullString()
        {
            return null;
        }

        private static string FormatIfParams(string format, object[] args)
        {
            if (format == null)
                return String.Empty;
            if (args == null || args.Length == 0)
                return format;
            return string.Format(format, args);
        }

        /// <summary>
        /// Appends a header to the <b>StringBuilder</b>.
        /// </summary>
        /// <param name="sb">The <b>StringBuilder</b> used for appending the header.</param>
        /// <param name="level"></param>
        /// <returns>The <b>StringBuilder</b> passed passed in.</returns>
        private static StringBuilder AppendHeader(StringBuilder sb, PluginTraceLevel level)
        {
            sb.AppendLine("[---");
            sb.AppendLine("[" + level.ToString() + "] @ " + DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"));
            return sb;
        }

        /// <summary>
        /// Appends a footer to the <b>StringBuilder</b>.
        /// </summary>
        /// <param name="sb">The <b>StringBuilder</b> used for appending the footer.</param>
        /// <returns>The <b>StringBuilder</b> passed passed in.</returns>
        private static StringBuilder AppendFooter(StringBuilder sb)
        {
            sb.AppendLine("---]\n");
            return sb;
        }

        /// <summary>
        /// Checks to determine if tracing is enabled.
        /// </summary>
        /// <returns><b>true</b> if tracing is enabled for the specific user.</returns>
        private bool TraceShouldBeWritten(PluginTraceLevel level)
        {
            return TraceShouldBeWritten() && (level <= this.Config.TraceLevel);
        }

        /// <summary>
        /// Checks to determine if tracing is enabled.
        /// </summary>
        /// <returns><b>true</b> if tracing is enabled for the specific user.</returns>
        private bool TraceShouldBeWritten()
        {
            // Trace is active/enabled if the configuration says so
            return ((this.Config.Enabled) && (this.Context != null));
        }

        /// <summary>
        /// Write the message to the trace log.
        /// </summary>
        /// <remarks>
        /// This is the <b>BIG DADDY</b> of all of the write methods.  All paths lead to enlightenment.
        /// </remarks>
        /// <param name="msg">The message to be written to the trace log.</param>
        private void WriteToBuffer(string msg)
        {
            // Get the buffer for the trace data
            if (this.Buffer != null)
            {
                this.Buffer.Append(msg);
            }
        }

        /// <summary>
        /// Transforms the plugin execution context into a human readable string suitable for writing to the trace log.
        /// </summary>
        /// <param name="sb">The string builder for the output string.</param>
        /// <param name="context">The plugin execution context to be transformed.</param>
        /// <param name="indent">A string that will be prepended to every line for indentation purposes.</param>
        private static void TransformContextToTraceString(StringBuilder sb, IPluginExecutionContext context, string indent)
        {
            try
            {
                sb.AppendLine(indent + "*** Plugin Execution Context ***");
                sb.AppendLine(indent + "BusinessUnitId:           " + context.BusinessUnitId.ToString());
                sb.AppendLine(indent + "CorrelationId:            " + context.CorrelationId.ToString());
                sb.AppendLine(indent + "Depth:                    " + context.Depth.ToString());
                sb.AppendLine(indent + "InitiatingUserId:         " + context.InitiatingUserId.ToString());
                sb.AppendLine(indent + "IsExecutingInOfflineMode: " + context.IsExecutingOffline.ToString());
                sb.AppendLine(indent + "IsInTransaction:          " + context.IsInTransaction.ToString());
                sb.AppendLine(indent + "IsOfflinePlayback:        " + context.IsOfflinePlayback.ToString());
                sb.AppendLine(indent + "IsolationMode:            " + context.IsolationMode.ToString());
                sb.AppendLine(indent + "MessageName:              " + context.MessageName.ToString());
                sb.AppendLine(indent + "Mode:                     " + context.Mode.ToString());
                sb.AppendLine(indent + "OperationCreatedOn:       " + context.OperationCreatedOn.ToString());
                sb.AppendLine(indent + "OrganizationId:           " + context.OrganizationId.ToString());
                sb.AppendLine(indent + "OrganizationName:         " + context.OrganizationName.ToString());
                sb.AppendLine(indent + "OwningExtension:          " + context.OwningExtension.ToString());
                sb.AppendLine(indent + "PrimaryEntityId:          " + context.PrimaryEntityId.ToString());
                sb.AppendLine(indent + "PrimaryEntityName:        " + context.PrimaryEntityName.ToString());
                sb.AppendLine(indent + "RequestId:                " + ((context.RequestId.HasValue) ? context.RequestId.ToString() : Guid.Empty.ToString()));
                sb.AppendLine(indent + "SecondaryEntityName:      " + context.SecondaryEntityName.ToString());
                sb.AppendLine(indent + "Stage:                    " + context.Stage.ToString());
                sb.AppendLine(indent + "UserId:                   " + context.UserId.ToString());
                sb.AppendLine();

                if (context.ParentContext != null)
                {
                    sb.AppendLine("ParentContext:");
                    TransformContextToTraceString(sb, context.ParentContext, indent + "    ");
                }
            }
            catch
            {
                // Don't really care, just catch the exception so things keep going
            }

            return;
        }

        /// <summary>
        /// Recursively build the string from the exception.
        /// </summary>
        /// <param name="sb">A StringBuilder for the output string.</param>
        /// <param name="e">The Exception used to build the string.</param>
        /// <param name="indent">A string that will be prepended to every line for indentation purposes.</param>
        private static void TransformExceptionToTraceString(StringBuilder sb, Exception e, string indent)
        {
            // Blow this popsicle stand if there is not an exception
            if (e == null)
            {
                return;
            }

            try
            {
                // Build the exception string
                sb.AppendLine(indent + "{" + e.GetType().FullName + "} from {" + e.Source + "} - " + e.Message);
                if (e is FaultException<OrganizationServiceFault>)
                {
                    FaultException<OrganizationServiceFault> fe = e as FaultException<OrganizationServiceFault>;
                    if (fe != null)
                    {
                        sb.AppendFormat("\n{0}Timestamp: {1}", indent, fe.Detail.Timestamp);
                        sb.AppendFormat("\n{0}Code: {1}", indent, fe.Detail.ErrorCode);
                        sb.AppendFormat("\n{0}Message: {1}", indent, fe.Detail.Message);
                        sb.AppendFormat("\n{0}Trace: {1}", indent, fe.Detail.TraceText);
                    }
                }
                else if (e is TimeoutException)
                {
                    TimeoutException te = e as TimeoutException;
                    if (te != null)
                    {
                        sb.AppendFormat("\n{0}Message: {1}", indent, te.Message);
                        sb.AppendFormat("\n{0}Stack Trace: {1}", indent, te.StackTrace);
                    }
                }
                sb.AppendLine(e.StackTrace.Replace("\n", "\n" + indent));

                // Recursively call
                TransformExceptionToTraceString(sb, e.InnerException, indent + "    ");
            }
            catch
            {
                // Don't really care, just catch the exception so things keep going
            }

            return;
        }

        /// <summary>
        /// Transforms a ParameterCollection into a human readable string suitable for writing to the trace log.
        /// </summary>
        /// <param name="sb">The StringBuilder for the output string.</param>
        /// <param name="pc">The ParameterCollection used to build the string.</param>
        /// <param name="indent">A string that will be prepended to every line for indentation purposes.</param>
        private static void TransformParameterCollectionToTraceString(StringBuilder sb, ParameterCollection pc, string indent)
        {
            string propFormatString = indent + "Name = {0}, Type = {1}, Value = {2}\n";

            // Blow this joint if the property bag is null
            if (pc == null)
            {
                return;
            }

            try
            {
                // Build a string from the property bag's properties

                var e = pc.GetEnumerator();
                foreach (var item in pc)
                {
                    if (item.Value is ValueType)
                    {
                        sb.AppendFormat(propFormatString, item.Key, item.Value.GetType().ToString(), item.Value.ToString());
                    }
                    else if (item.Value is Entity)
                    {
                        sb.AppendFormat(propFormatString, item.Key, item.Value.GetType().ToString(), "{Entity}");
                        PluginTraceLog.TransformEntityToTraceString(sb, item.Value as Entity, indent + "    ");
                    }
                    else
                    {
                        sb.AppendFormat(propFormatString, item.Key, item.Value.GetType().ToString(), "{Object}");
                        PluginTraceLog.TransformObjectToTraceString(sb, item.Value, indent + "    ");
                    }
                }
            }
            catch
            {
                // Don't really care, just catch the exception so things keep going
            }
        }

        /// <summary>
        /// Transforms a EntityImageCollection into a human readable string suitable for writing to the trace log.
        /// </summary>
        /// <param name="sb">The StringBuilder for the output string.</param>
        /// <param name="eic">The EntityImageCollection used to build the string.</param>
        /// <param name="indent">A string that will be prepended to every line for indentation purposes.</param>
        private static void TransformEntityImageCollectionToTraceString(StringBuilder sb, EntityImageCollection eic, string indent)
        {
            string itemFormatString = indent + "EntityImage = {0}\n";


            // Blow this joint if the property bag is null
            if (eic == null)
            {
                return;
            }

            try
            {
                // Build a string from the property bag's properties

                var e = eic.GetEnumerator();
                foreach (var item in eic)
                {
                    sb.AppendFormat(itemFormatString, item.Key);
                    TransformEntityToTraceString(sb, item.Value, indent + "    ");
                }
            }
            catch
            {
                // Don't really care, just catch the exception so things keep going
            }
        }

        /// <summary>
        /// Builds a human readable string from the Entity that is suitable for output to the trace log.
        /// </summary>
        /// <param name="sb">A StringBuilder for the building the output string.</param>
        /// <param name="de">The Entity to be transformed to a string.</param>
        /// <param name="indent">A string that will be prepended to every line for indentation purposes.</param>
        private static void TransformEntityToTraceString(StringBuilder sb, Entity entity, string indent)
        {
            string formatString = indent + "Name = {0}, Type = {1}, ValueEntity = {2}, Value = {3}\n";
            string formatStringEntityReference = indent + "Name = {0}, Type = {1}, ValueEntity = {2}, Value = {3}, LogicalName = {4}, Name = {5}\n";

            // Make sure the dynamic entity is not null
            if (entity == null)
            {
                return;
            }

            try
            {
                // Build the string from the entities properties and values
                sb.AppendFormat(indent + "*** Entity ({0}) - Begin ***\n", entity.LogicalName);
                foreach (var item in entity.Attributes)
                {
                    if (item.Value is EntityReference)
                    {
                        sb.AppendFormat(formatStringEntityReference, new object[] { item.Key, item.Value.GetType().ToString(), "n/a", ((EntityReference)item.Value).Id, ((EntityReference)item.Value).LogicalName, ((EntityReference)item.Value).Name });
                    }
                    else if (item.Value is Money)
                    {
                        sb.AppendFormat(formatString, new object[] { item.Key, item.Value.GetType().ToString(), "n/a", ((Money)item.Value).Value });
                    }
                    else if (item.Value is OptionSetValue)
                    {
                        sb.AppendFormat(formatString, new object[] { item.Key, item.Value.GetType().ToString(), "n/a", ((OptionSetValue)item.Value).Value });
                    }
                    else if (item.Value is EntityCollection)
                    {
                        foreach (Entity entityItem in ((EntityCollection)item.Value).Entities)
                        {
                            sb.AppendFormat(indent + "Name = {0}, Id = {1}\n", entityItem.LogicalName, entityItem.Id);
                            PluginTraceLog.TransformEntityToTraceString(sb, entity, indent + "    ");
                        }
                    }
                    else if (item.Value == null)
                    {
                        sb.AppendFormat(formatString, new object[] { item.Key, "<NULL>", "n/a", "<NULL>" });
                    }
                    else
                    {
                        sb.AppendFormat(formatString, new object[] { item.Key, item.Value.GetType().ToString(), "n/a", item.Value });
                    }
                }
                sb.AppendFormat(indent + "*** Entity ({0}) - End ***\n", entity.LogicalName);
                sb.AppendLine();
            }
            catch
            {
                // Don't really care, just catch the exception so things keep going
            }
        }

        /// <summary>
        /// Builds a human readable string from the object that is suitable for output to the trace log.
        /// </summary>
        /// <remarks>
        /// This method will not output any data for object properties that are indexed properties and if the
        /// property is an array it simply calls the ToString method on the property.
        /// </remarks>
        /// <param name="sb">A StringBuilder for building the output string.</param>
        /// <param name="o">The object to be transformed to a string.</param>
        /// <param name="indent">A string that will be prepended to every line for indentation purposes.</param>
        private static void TransformObjectToTraceString(StringBuilder sb, Object o, string indent)
        {
            string propFormatString = indent + "Name = {0}, Type = {1}, Value = {2}\n";
            object[] arrayIndex = new object[1] { 0 };

            // Make sure the dynamic entity is not null
            if (o == null)
            {
                return;
            }

            try
            {
                // Build the string from the entities properties and values
                sb.AppendFormat(indent + "*** Object ({0})- Begin ***\n", o.GetType().ToString());

                PropertyInfo[] props = o.GetType().GetProperties();
                foreach (PropertyInfo p in props)
                {
                    // Handle anything that is not an indexed property by calling ToString on the value
                    if (p.GetValue(o, null) == null)
                    {
                        sb.AppendFormat(propFormatString, p.Name, p.PropertyType.ToString(), "{null}");
                    }
                    else
                    {
                        sb.AppendFormat(propFormatString, p.Name, p.PropertyType.ToString(), p.GetValue(o, null).ToString());
                    }

                    //// Handle the properties based on the metadata for the property, we are ignoring indexed properties at this time
                    ////if (p.PropertyType.IsClass && !p.PropertyType.IsArray && (p.GetIndexParameters().Length == 0))
                    //if (p.PropertyType.IsClass)
                    //{
                    //    // This case handles properties that are classes, not array, and not index properties
                    //    sb.AppendFormat(propFormatString, p.Name, p.PropertyType.ToString(), "{Object}");
                    //    PluginTraceLog.TransformObjectToTraceString(sb, p.GetValue(o, null), indent + "    ");
                    //}
                    ////else if (p.GetIndexParameters().Length == 0)
                    //else
                    //{
                    //    // Handle anything that is not an indexed property by calling ToString on the value
                    //    if (p.GetValue(o, null) == null)
                    //    {
                    //        sb.AppendFormat(propFormatString, p.Name, p.PropertyType.ToString(), "{null}");
                    //    }
                    //    else
                    //    {
                    //        sb.AppendFormat(propFormatString, p.Name, p.PropertyType.ToString(), p.GetValue(o, null).ToString());
                    //    }
                    //}
                }
                sb.AppendFormat(indent + "*** Object ({0}) - End ***\n", o.GetType().ToString());
                sb.AppendLine();
            }
            catch
            {
                // Don't really care, just catch the exception so things keep going
            }
        }

        /// <summary>
        /// Writes the data in the buffer to the event log.
        /// </summary>
        /// <param name="traceContext">The trace context containing the data to be written.</param>
        private void WriteBufferToEventLog()
        {
            // Make sure we are configured to write to the log
            if (!this.Config.TraceToEventLog)
            {
                return;
            }

            try
            {
                // Write the message to the event log, taking care not too write an entry that
                // exceeds the limits of an event log entry (32766)
                string msg = this.Buffer.ToString();
                string s;
                for (int i = 0; i < msg.Length; i += 30000)
                {
                    if ((i + 30000) > msg.Length)
                    {
                        s = "*** " + this.PluginName + " Log ***\n\n" + msg.Substring(0);
                    }
                    else
                    {
                        s = "*** " + this.PluginName + " Log ***\n\n" + msg.Substring(0, 30000) + "\n\n*** THIS MESSAGE IS CONTINUED IN THE NEXT EVENT LOG ENTRY ***";
                    }

                    EventLog.WriteEntry(CrmEventLogSource, s, EventLogEntryType.Information);
                }
            }
            catch
            {
                // Not too concerned, yeah it sucks that the message didn't get logged to the event log, but the world will not end because of it.
            }
        }

        /// <summary>
        /// Writes the data in the buffer to the tracing service.
        /// </summary>
        /// <param name="traceContext">The trace context containing the data to be written.</param>
        private void WriteBufferToTraceService()
        {
            // Make sure we are configured to write to the log
            if (!this.Config.TraceToTracingService)
            {
                return;
            }

            try
            {
                // Write the message to the tracing service.
                this.TracingService.Trace(this.Buffer.ToString());
            }
            catch
            {
                // Not too concerned, yeah it sucks that the message didn't get logged to the tracing service, but the world will not end because of it.
            }
        }

        /// <summary>
        /// Writes the data in the buffer to the path specified in the trace configuration.
        /// </summary>
        /// <param name="traceContext"></param>
        private void WriteBufferToNote()
        {
            // Make sure we are configured to write to the log
            if (!this.Config.TraceToAnnotation)
            {
                return;
            }

            try
            {
                // Build the attached note
                Entity traceLogNote = new Entity("annotation");
                traceLogNote.Id = Guid.NewGuid();
                traceLogNote["filename"] = "trace-data.txt";
                traceLogNote["isdocument"] = true;
                traceLogNote["subject"] = string.Format("Trace Data | {0} | {1:yyyyMMddTHH:mm:ss}", this.PluginName, DateTime.Now);
                traceLogNote["documentbody"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(this.Buffer.ToString()));

                // Create the new record
                this.OrgService.Create(traceLogNote);
            }
            catch
            {
                // Not too concerned, yeah it sucks that the message didn't get logged to the event log, but the world will not end because of it.
            }
        }

        #endregion
    }

}
