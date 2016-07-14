using System;
namespace Hsl.Xrm.Sdk.Plugin.Service
{
	interface IMultiServicePluginAction
	{
		object ExecuteServiceAction(string data);
		IServicePluginContext ServiceContext { get; }
	}
}
