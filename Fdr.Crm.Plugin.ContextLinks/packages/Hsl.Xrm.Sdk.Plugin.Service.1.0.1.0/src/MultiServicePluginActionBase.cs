using Hsl.Xrm.Sdk.Plugin.Service.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hsl.Xrm.Sdk.Plugin.Service
{
	public abstract class MultiServicePluginActionBase<TReq, TResp> : IMultiServicePluginAction
	{
		#region Constructors

		public MultiServicePluginActionBase(IServicePluginContext context)
		{
			this.ServiceContext = context;
		}

		#endregion

		public abstract TResp ExecuteAction(TReq request);

		public IServicePluginContext ServiceContext { get; private set; }

		public object ExecuteServiceAction(string data)
		{
			var serializer = GetSerializer();
			TReq req;
			if (String.IsNullOrEmpty(data))
			{
				req = default(TReq);
			}
			else if (typeof(TReq) == typeof(String))
			{
				req = (TReq)(object)data;
			}
			else
			{
				req = serializer.Parse<TReq>(data);
			}

			var resp = ExecuteAction(req);
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

		public virtual ISerializer GetSerializer()
		{
			return new JsonNetSerializer();
		}
	}
}
