using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;


namespace Hsl.Xrm.Sdk.Plugin.Service.Serializers
{
	public class JsonNetSerializer : ISerializer
	{
		private readonly JsonSerializerSettings settings;

		public JsonNetSerializer()
			: this(new JsonSerializerSettings
			{
				DateParseHandling = DateParseHandling.None,
				ConstructorHandling = ConstructorHandling.Default,
			})
		{
		}

		public JsonNetSerializer(JsonSerializerSettings settings)
		{
			this.settings = settings;
		}

		public T Parse<T>(string data)
		{
			return JsonConvert.DeserializeObject<T>(data, settings);
		}

		public string Serialize<T>(T data)
		{
			return JsonConvert.SerializeObject(data, settings);
		}
	}

}
