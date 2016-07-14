using Microsoft.Xrm.Sdk;
using System;
namespace Hsl.Xrm.Sdk.Plugin
{
	public interface IEventContext
	{
		string ActionTypeName { get; }
		void Assert(bool condition, string message, params object[] args);
		Exception BuildInnerException(Exception ex);
		bool CanSendException(Exception ex);
		IExecutionContext ExecutionContext { get; }
		IOrganizationService GetOrgService(Guid? userId);
		T GetSharedVariable<T>(string name, bool includeParentContexts = true, T defaultValue = default(T));
		T GetTargetValue<T>(string attributeName);
		IOrganizationService InitiatingUserOrgService { get; }
		bool IsExecutingAsynchronously { get; }
		bool IsExecutingInSandbox { get; }
		bool IsExecutingSynchronously { get; }
		IOrganizationService SystemOrgService { get; }
		Entity TargetInput { get; }
		EntityReference TargetInputEntityReference { get; }
		Entity TargetPostImage { get; }
		Entity TargetPreImage { get; }
		void Throw(string message, Exception inner, params object[] args);
		void Trace(string message, params object[] args);
		void TraceException(string message, Exception ex, params object[] args);
		ITracingService TracingService { get; }
		IOrganizationService UserOrgService { get; }
	}
}
