using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hsl.Xrm.Sdk.Plugin.Service
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class ServicePluginOperationAttribute : Attribute
	{
		public ServicePluginOperationAttribute()
		{
		}
		public ServicePluginOperationAttribute(string operationName)
		{
			Name = operationName;
		}

		public string Name { get; set; }
		public bool Async { get; set; }
	}
}
