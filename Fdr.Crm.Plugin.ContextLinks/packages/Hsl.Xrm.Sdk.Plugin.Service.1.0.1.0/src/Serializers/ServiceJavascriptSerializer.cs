using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Hsl.Xrm.Sdk.Plugin.Service.Serializers
{
	/// <summary>
	/// Wrapper of JavascriptSerializer that implements ISerializer
	/// </summary>
	public class ServiceJavascriptSerializer : ISerializer
	{
		public int MaxJsonLength { get; set; }
		public int RecursionLimit { get; set; }
		private readonly JavaScriptTypeResolver _typeResolver;

		public ServiceJavascriptSerializer()
		{
			// Default to 128 MB
			MaxJsonLength = 1024 * 1024 * 128;
			RecursionLimit = 100;
		}

		public ServiceJavascriptSerializer(JavaScriptTypeResolver typeResolver)
			: this()
		{
			_typeResolver = typeResolver;
		}

		public T Parse<T>(string data)
		{
			var serializer = GetSerializer();
			return serializer.Deserialize<T>(data);
		}

		public string Serialize<T>(T data)
		{
			var serializer = GetSerializer();
			return serializer.Serialize(data);
		}

		private JavaScriptSerializer GetSerializer()
		{
			var ser = new JavaScriptSerializer(_typeResolver);
			ser.MaxJsonLength = MaxJsonLength;
			ser.RecursionLimit = RecursionLimit;
			return ser;
		}
	}
}
