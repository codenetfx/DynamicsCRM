using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hsl.Xrm.Sdk.Plugin.Service
{
	public class ServicePluginDataInfo
	{
		public string Name { get; set; }
		public string DataAttribute { get; set; }
		public string Data { get; set; }
		public Entity Target { get; set; }
	}

}
