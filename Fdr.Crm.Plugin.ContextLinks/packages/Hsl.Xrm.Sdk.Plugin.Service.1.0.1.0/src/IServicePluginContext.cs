using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hsl.Xrm.Sdk.Plugin.Service
{
	public interface IServicePluginContext : IEventContext, IPluginEventContext
	{
		ServicePluginOperationType OperationType { get; }
		ServicePluginDataInfo ServiceDataInfo { get; }
	}
}
