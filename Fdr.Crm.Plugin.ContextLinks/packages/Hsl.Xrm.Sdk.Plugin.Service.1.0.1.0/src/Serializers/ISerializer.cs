using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hsl.Xrm.Sdk.Plugin.Service.Serializers
{
	public interface ISerializer
	{
		T Parse<T>(string data);
		string Serialize<T>(T data);
	}

}
