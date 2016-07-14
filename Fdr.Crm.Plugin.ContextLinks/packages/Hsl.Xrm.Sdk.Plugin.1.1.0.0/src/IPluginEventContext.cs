using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hsl.Xrm.Sdk.Plugin
{
	public interface IPluginEventContext : IEventContext
	{
		PluginConfigurationManager ConfigManager { get; }
		PluginTraceConfiguration TraceConfig { get; }
		PluginTraceLog TraceLog { get; }
		IServiceProvider ServiceProvider { get; }
		IPluginExecutionContext PluginContext { get; }

		void Initialize(string pluginName, IServiceProvider serviceProvider, PluginConfigurationManager configManager, PluginTraceConfiguration traceConfig);
	}
}
