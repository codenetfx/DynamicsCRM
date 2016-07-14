using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Hsl.Xrm.Sdk.Plugin
{
	public abstract class PluginBase : IPlugin
	{
		#region Variables

		private PluginConfigurationManager ConfigManager { get; set; }
		private PluginTraceConfiguration TraceConfig { get; set; }

		#endregion

		#region Constructors

		public PluginBase()
		{
			this.ConfigManager = new PluginConfigurationManager();
			this.TraceConfig = PluginTraceConfiguration.Default;
			this.Context = null;
		}

		public PluginBase(string unsecureConfig, string secureConfig)
			: this()
		{
			// Initialize the configuration
			this.ConfigManager.InitializeConfig(unsecureConfig);
			this.ConfigManager.InitializeSecureConfig(secureConfig);
			this.TraceConfig = PluginTraceConfiguration.CreateFromConfiguration(unsecureConfig, secureConfig);
		}

		#endregion

		#region Properties

		public IPluginEventContext Context { get; private set; }

		#endregion

		protected virtual IPluginEventContext CreateContext()
		{
			return new PluginEventContext();
		}

		public abstract void ExecuteAction();



		public void Execute(IServiceProvider serviceProvider)
		{
			var instance = Activator.CreateInstance(this.GetType()) as PluginBase;
			var ctx = this.CreateContext();
			ctx.Initialize(this.GetType().FullName, serviceProvider, ConfigManager, TraceConfig);
			instance.Context = ctx;

			try
			{
				instance.ExecuteAction();
			}
			catch (InvalidPluginExecutionException ex)
			{
				var msg = "An invalid operation has occurred during the execution of the plugin.";
				instance.Context.TraceException(msg, ex);
				if (instance.Context.CanSendException(ex))
					throw;
				else
					instance.Context.Throw(ex.Message, instance.Context.BuildInnerException(ex));
			}
			catch (FaultException<OrganizationServiceFault> ex)
			{
				var msg = "An unhandled organization service fault occurred during execution of the plugin.";
				instance.Context.TraceException(msg, ex);
				instance.Context.Throw(msg, instance.Context.BuildInnerException(ex));
			}
			catch (TimeoutException ex)
			{
				var msg = "A timeout has occurred during the execution of the plugin.";
				instance.Context.TraceException(msg, ex);
				instance.Context.Throw(msg, instance.Context.BuildInnerException(ex));
			}
			catch (Exception ex)
			{
				var msg = "An unhandled exception has occurred during execution of the plugin.";
				instance.Context.TraceException(msg, ex);
				instance.Context.Throw(msg, instance.Context.BuildInnerException(ex));
			}
			finally
			{
                instance.Context.TraceLog.Save();
			}
		}

	}
}
