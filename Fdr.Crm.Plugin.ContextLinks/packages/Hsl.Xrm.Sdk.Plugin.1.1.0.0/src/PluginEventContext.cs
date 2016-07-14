using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Hsl.Xrm.Sdk.Plugin
{
    public class PluginEventContext : EventContext, IEventContext, IPluginEventContext
    {
		public PluginEventContext() { }

		public virtual void Initialize(string pluginName, IServiceProvider serviceProvider, PluginConfigurationManager configManager, PluginTraceConfiguration traceConfig)
		{
			if (serviceProvider == null) { throw new ArgumentNullException("serviceProvider"); }
			if (configManager == null) { throw new ArgumentNullException("configManager"); }
			if (traceConfig == null) { throw new ArgumentNullException("traceConfig"); }

			this.ActionTypeName = pluginName;
			this.ServiceProvider = serviceProvider;
			this.ConfigManager = configManager;
			this.TraceConfig = traceConfig;
			this.TraceLog = PluginTraceLog.Create(pluginName, traceConfig, serviceProvider);
		}

		#region Context Helpers

		/// <summary>
		/// Gets the configuration manager for the plugin.
		/// </summary>
		public PluginConfigurationManager ConfigManager { get; private set; }

		/// <summary>
		/// Gets the trace configuration for the plugin.
		/// </summary>
		public PluginTraceConfiguration TraceConfig { get; private set; }

		/// <summary>
		/// Gets the trace log for writing tracing information.
		/// </summary>
		public PluginTraceLog TraceLog { get; private set; }

		/// <summary>
		/// Gets the service provider for the plugin execution instance.
		/// </summary>
		public IServiceProvider ServiceProvider { get; private set; }

		/// <summary>
		/// Gets the execution context for the plugin.
		/// </summary>
		public IPluginExecutionContext PluginContext
		{
			get
			{
				if (_PluginContext != null) { return _PluginContext; }
				_PluginContext = this.GetExecutionContext() as IPluginExecutionContext;
				return _PluginContext;
			}
		}
		private IPluginExecutionContext _PluginContext = null;

		#endregion

		#region OperationContext Overrides

		protected override IExecutionContext GetExecutionContext()
		{
			if (this.ServiceProvider == null)
			{
				throw new InvalidOperationException("The context cannot be accessed until a valid ServiceProvider exists.");
			}

			return this.ServiceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
		}

		protected override IOrganizationServiceFactory GetOrganizationServiceFactory()
		{
			if (this.ServiceProvider == null)
			{
				throw new InvalidOperationException("The organization service factory cannot be accessed until a valid ServiceProvider exists.");
			}
			var factory = this.ServiceProvider.GetService(typeof(IOrganizationServiceFactory)) as IOrganizationServiceFactory;
			return factory;
		}

		protected override IExecutionContext GetParentContext(IExecutionContext ctx)
		{
			var pluginCtx = ctx as IPluginExecutionContext;
			if (pluginCtx == null) { return null; }
			return pluginCtx.ParentContext;
		}

		protected override ITracingService GetTracingService()
		{
			if (this.ServiceProvider == null)
			{
				throw new InvalidOperationException("The tracing service cannot be accessed until a valid ServiceProvider exists.");
			}
			return this.ServiceProvider.GetService(typeof(ITracingService)) as ITracingService;
		}

		#endregion
	}
}
