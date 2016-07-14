using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hsl.Xrm.Sdk.Plugin.Service.Serializers;


namespace Hsl.Xrm.Sdk.Plugin.Service
{
    public abstract class ServicePluginBase<TReq, TResp> : ServicePluginBase
	{
		#region Constructors

		protected ServicePluginBase() { }
		protected ServicePluginBase(string config, string secureConfig) : base(config, secureConfig) { }

		#endregion

		public abstract TResp ExecuteService(TReq request);

		#region ServicePluginBase Overrides & Helpers

		/// <summary>
		/// Handles plugin service execution by processing the data, calling the action and processing the result.
		/// </summary>
		public sealed override object ExecuteService(string data)
		{
			var serializer = GetSerializer();
			TReq req;
			if (String.IsNullOrEmpty(data))
			{
				req = default(TReq);
			}
			else if(typeof(TReq) == typeof(String))
			{
				req = (TReq) (object) data;
			}
			else
			{
				req = serializer.Parse<TReq>(data);
			}

			var resp = ExecuteService(req);
			if (resp is ResultSetResponse)
			{
				return resp;
			}
			if (typeof(TResp) == typeof(string))
			{
				return resp;
			}

			return serializer.Serialize<TResp>(resp);
		}

		/// <summary>
		/// Gets the serializer to be used when serializing/deserializing the service plugin data.
		/// </summary>
		public virtual ISerializer GetSerializer()
		{
			return new JsonNetSerializer();
		}

		#endregion
	}
}
