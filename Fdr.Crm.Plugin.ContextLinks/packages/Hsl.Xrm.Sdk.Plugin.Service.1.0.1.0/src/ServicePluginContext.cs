using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Hsl.Xrm.Sdk.Plugin.Service
{
	public class ServicePluginContext : PluginEventContext, IServicePluginContext, IPluginEventContext, IEventContext
	{
		#region Constructors

		public ServicePluginContext() : base() { }

		#endregion

		#region PluginEventContext Overrides

		public override void Initialize(string pluginName, IServiceProvider serviceProvider, PluginConfigurationManager configManager, PluginTraceConfiguration traceConfig)
		{
			base.Initialize(pluginName, serviceProvider, configManager, traceConfig);
		}

		#endregion

		#region Properties

		public ServicePluginOperationType OperationType
		{
			get
			{
				if (_OperationType != null) { return _OperationType.Value; }

				var msg = ExecutionContext.MessageName;
				if (msg.Equals("Create"))
				{
					_OperationType = ServicePluginOperationType.Create;
				}
				else if (msg.Equals("RetrieveMultiple"))
				{
					_OperationType = ServicePluginOperationType.RetrieveMultiple;
				}
				else
				{
					_OperationType = ServicePluginOperationType.Action;
				}

				return _OperationType.Value;
			}
		}
		private ServicePluginOperationType? _OperationType;

		public ServicePluginDataInfo ServiceDataInfo { get; set; }

		#endregion

	}
}
