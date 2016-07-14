using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hsl.Xrm.Sdk.Plugin.Service
{
	public abstract class MultiServicePluginBase : ServicePluginBase
	{
		#region Variables

		#endregion

		#region Constructors/Destructors

		public MultiServicePluginBase() : base() { }
		public MultiServicePluginBase(string unsecureConfig, string secureConfig) : base(unsecureConfig, secureConfig) { }

		#endregion

		#region Properties

		#endregion

		protected abstract IEnumerable<Type> GetRegisteredTypes();

		public override object ExecuteService(string data)
		{
			var ctx = ServiceContext;
			if ((ctx.OperationType == ServicePluginOperationType.Create) && ctx.IsExecutingAsynchronously && GetIsComplete())
			{
				return new ProcessingNotCompleteResult(null);// Will not process, already complete. 
			}

			var serviceInfo = ResolveService(ctx.ServiceDataInfo.Name);
			var serviceType = ctx.IsExecutingSynchronously ? serviceInfo.Sync : serviceInfo.Async;
			var result = CreateAndExecuteService(serviceType, data);

			if (ctx.IsExecutingSynchronously && (serviceInfo.Async != null) && (ctx.OperationType == ServicePluginOperationType.Create) &&
				!GetIsComplete() && !(result is ProcessingNotCompleteResult))
			{
				return new ProcessingNotCompleteResult(result);
			}

			return result;
		}

		#region Helpers

		private Dictionary<string, MultiServicePluginActionInfo> BuildServiceInfoDictionary()
		{
			var ctx = ServiceContext;
			Dictionary<string, MultiServicePluginActionInfo> services = GetRegisteredTypes().Select(t =>
			{
				var attr = GetPluginOperationAttribute(t);

				if (!typeof(IMultiServicePluginAction).IsAssignableFrom(t))
				{
					ctx.Throw("Type '{0}' must implement IService to be registered on the multiserviceplugin.", null, t.FullName);
				}

				return new
				{
					Type = t,
					Name = attr == null || String.IsNullOrEmpty(attr.Name) ? t.Name : attr.Name,
					Async = attr != null && attr.Async,
				};
			})
			.GroupBy(x => x.Name)
			.ToDictionary(x => x.Key, x =>
			{
				var serviceInfo = new MultiServicePluginActionInfo();
				try
				{
					serviceInfo.Async = x.Where(y => y.Async).Select(y => y.Type).SingleOrDefault();
					serviceInfo.Sync = x.Where(y => !y.Async).Select(y => y.Type).SingleOrDefault();
				}
				catch (InvalidOperationException)
				{
					ctx.Throw("Only a single service can be registered to the name " + x.Key, null);
				}
				return serviceInfo;
			}, null);

			return services;
		}

		private object CreateAndExecuteService(Type serviceType, string data)
		{
			if (serviceType == null)
			{
				return new ProcessingNotCompleteResult(null);
			}

			IMultiServicePluginAction instance = null;
			try
			{
				instance = (IMultiServicePluginAction)Activator.CreateInstance(serviceType, this.ServiceContext);
			}
			catch (MissingMethodException)
			{
				Context.Throw("{0} must have a public constructor that takes an IServicePluginContext parameter.", null, serviceType.FullName);
			}

			var result = instance.ExecuteServiceAction(data);
			return result;
		}

		private bool GetIsComplete()
		{
			return Context.TargetInput.GetFieldValueOrDefault<bool>(GetPrefixedFieldName(ServicePluginBase.ServiceFieldName_IsComplete), false);
		}

		private static ServicePluginOperationAttribute GetPluginOperationAttribute(Type type)
		{
			return (ServicePluginOperationAttribute)type
				.GetCustomAttributes(typeof(ServicePluginOperationAttribute), true)
				.FirstOrDefault();
		}

		private Dictionary<string, MultiServicePluginActionInfo> GetServiceActionInfo()
		{
			return _ServiceActionInfo.GetOrAdd(this.GetType(), (type) => BuildServiceInfoDictionary());
		}
		private static readonly ConcurrentDictionary<Type, Dictionary<string, MultiServicePluginActionInfo>> _ServiceActionInfo = new ConcurrentDictionary<Type, Dictionary<string, MultiServicePluginActionInfo>>();

		private bool HasIMultiServicePluginActionConstructor(System.Reflection.ConstructorInfo x)
		{
			var parameters = x.GetParameters().ToList();
			return (parameters.Count == 1) && typeof(IMultiServicePluginAction).IsAssignableFrom(parameters[0].ParameterType);
		}

		private MultiServicePluginActionInfo ResolveService(string name)
		{
			var ctx = ServiceContext;
			var dict = GetServiceActionInfo();
			if (!dict.ContainsKey(name))
			{
				ctx.Throw("Type '{0}' is not registered. ", null, name);
			}
			return dict[name];
		}

		public class MultiServicePluginActionInfo
		{
			public Type Sync { get; set; }
			public Type Async { get; set; }
		}

		#endregion
	}
}
