using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hsl.Xrm.Sdk.Plugin
{
	public abstract class EventContext : IEventContext
	{
		public EventContext() { }

		protected abstract IExecutionContext GetExecutionContext();
		protected abstract IOrganizationServiceFactory GetOrganizationServiceFactory();
		protected abstract ITracingService GetTracingService();
		protected abstract IExecutionContext GetParentContext(IExecutionContext ctx);

		#region Context Helpers

		public string ActionTypeName { get; protected set; }

		/// <summary>
		/// Gets the execution context for the event execution instance.
		/// </summary>
		public IExecutionContext ExecutionContext
		{
			get
			{
				if (_ExecutionContext != null) { return _ExecutionContext; }
				_ExecutionContext = this.GetExecutionContext();
				return _ExecutionContext;
			}
		}
		private IExecutionContext _ExecutionContext = null;

		/// <summary>
		/// Indicates if executing asynchronously.
		/// </summary>
		public bool IsExecutingAsynchronously
		{
			get
			{
				if (_IsExecutingAsynchronously != null) { return _IsExecutingAsynchronously.Value; }

				_IsExecutingAsynchronously = ((ExecutionContext != null) && (ExecutionContext.Mode == (int)SdkMessageProcessingStepMode.Asynchronous));
				return _IsExecutingAsynchronously.Value;
			}
		}
		private bool? _IsExecutingAsynchronously;

		/// <summary>
		/// Indicates if executing synchronously.
		/// </summary>
		public bool IsExecutingSynchronously
		{
			get
			{
				if (_IsExecutingSynchronously != null) { return _IsExecutingSynchronously.Value; }

				_IsExecutingSynchronously = ((ExecutionContext != null) && (ExecutionContext.Mode == (int)SdkMessageProcessingStepMode.Synchronous));
				return _IsExecutingSynchronously.Value;
			}
		}
		private bool? _IsExecutingSynchronously;

		/// <summary>
		/// Indicates if executing in Sandbox isolation mode.
		/// </summary>
		public bool IsExecutingInSandbox
		{
			get
			{
				if (_IsExecutingInSandbox != null) { return _IsExecutingInSandbox.Value; }

				_IsExecutingInSandbox = ((ExecutionContext != null) && (ExecutionContext.IsolationMode == (int)PluginAssemblyIsolationMode.Sandbox));
				return _IsExecutingInSandbox.Value;
			}
		}
		private bool? _IsExecutingInSandbox;

		/// <summary>
		/// Gets an organization service using the system user's context.
		/// </summary>
		public IOrganizationService SystemOrgService
		{
			get
			{
				if (_SystemOrgService != null) { return _SystemOrgService; }

				IOrganizationServiceFactory factory = this.GetOrganizationServiceFactory();
				_SystemOrgService = factory.CreateOrganizationService(null);

				return _SystemOrgService;
			}
		}
		private IOrganizationService _SystemOrgService = null;

		/// <summary>
		/// Gets an organization service using the user's context.
		/// </summary>
		public IOrganizationService UserOrgService
		{
			get
			{
				if (_UserOrgService != null) { return _UserOrgService; }

				IOrganizationServiceFactory factory = this.GetOrganizationServiceFactory();
				_UserOrgService = factory.CreateOrganizationService(this.ExecutionContext.UserId);

				return _UserOrgService;
			}
		}
		private IOrganizationService _UserOrgService = null;

		/// <summary>
		/// Gets an organization service using the initiating user's context.
		/// </summary>
		public IOrganizationService InitiatingUserOrgService
		{
			get
			{
				if (_InitiatingUserOrgService != null) { return _InitiatingUserOrgService; }

				IOrganizationServiceFactory factory = this.GetOrganizationServiceFactory();
				_SystemOrgService = factory.CreateOrganizationService(this.ExecutionContext.InitiatingUserId);

				return _InitiatingUserOrgService;
			}
		}
		private IOrganizationService _InitiatingUserOrgService = null;

		/// <summary>
		/// Gets the <b>Target</b> <b>Entity</b> if one exists in the <b>InputParameters</b> of the plugins context.
		/// </summary>
		public Entity TargetInput
		{
			get
			{
				if (_TargetInput != null) { return _TargetInput; }

				_TargetInput = ((ExecutionContext == null) || (ExecutionContext.InputParameters == null) || !ExecutionContext.InputParameters.Contains("Target")) ? null : ExecutionContext.InputParameters.GetItemAs<Entity>("Target");

				return _TargetInput;
			}
		}
		private Entity _TargetInput;

		/// <summary>
		/// Gets the <b>Target</b> <b>EntityReference</b> if one exists in the <b>InputParameters</b> of the plugins context.
		/// </summary>
		public EntityReference TargetInputEntityReference
		{
			get
			{
				if (_TargetInputEntityReference != null) { return _TargetInputEntityReference; }

				_TargetInputEntityReference = ((ExecutionContext == null) || (ExecutionContext.InputParameters == null) || !ExecutionContext.InputParameters.Contains("Target")) ? null : ExecutionContext.InputParameters.GetItemAs<EntityReference>("Target");
				return _TargetInputEntityReference;
			}
		}
		private EntityReference _TargetInputEntityReference;

		/// <summary>
		/// Gets the <b>Target</b> <b>Entity</b> named <i>PreImage</i>from the <b>PreEntityImages</b> if one exists.
		/// </summary>
		public Entity TargetPreImage
		{
			get
			{
				if (_TargetPreImage != null) { return _TargetPreImage; }

				_TargetPreImage = ((ExecutionContext == null) || (ExecutionContext.PreEntityImages == null) || !ExecutionContext.PreEntityImages.Contains("Target") ? null : ExecutionContext.PreEntityImages["Target"]);
				return _TargetPreImage;
			}
		}
		private Entity _TargetPreImage;

		/// <summary>
		/// Gets the <b>Target</b> <b>Entity</b> named <i>PostImage</i>from the <b>PostEntityImages</b> if one exists.
		/// </summary>
		public Entity TargetPostImage
		{
			get
			{
				if (_TargetPostImage != null) { return _TargetPostImage; }

				_TargetPostImage = ((ExecutionContext == null) || (ExecutionContext.PostEntityImages == null) || !ExecutionContext.PostEntityImages.Contains("Target") ? null : ExecutionContext.PostEntityImages["Target"]);
				return _TargetPostImage;
			}
		}
		private Entity _TargetPostImage;

		/// <summary>
		/// Gets the Tracing service from the plugin context.
		/// </summary>
		public ITracingService TracingService
		{
			get
			{
				if (_TracingService != null) { return _TracingService; }

				_TracingService = this.GetTracingService();
				return _TracingService;
			}
		}
		private ITracingService _TracingService = null;

		/// <summary>
		/// Gets a value from the InputParameters or PreImage Target parameter of the IExecutionContext
		/// If the value is not found in either of the collections then a value of <b>default(T)</b> will be returned.
		/// </summary>
		/// <returns>
		/// If the property exists in the InputParameters Target parameter, then the parameter contained in the InputParameters or the default value specified will be returned.
		/// If the property does not exist in the InputParameters Target parameter, then the parameter contained in the PreImage Target property or the default(T) value will be returned.
		/// If the property does not exist in either the InputParameters or PreImage Target parameter, then the default(T) value will be returned.
		/// If both the InputParameters and PreImage do not contain a Target property then the default(T) value will be returned.
		/// </returns>
		public T GetTargetValue<T>(string attributeName)
		{
			if ((TargetInput != null) && TargetInput.Contains(attributeName))
			{
				return TargetInput.GetAttributeValue<T>(attributeName);
			}

			if ((TargetPreImage != null) && TargetPreImage.Contains(attributeName))
			{
				return TargetPreImage.GetAttributeValue<T>(attributeName);
			}

			return default(T);
		}

		/// <summary>
		/// Gets an organization service using the specified user.
		/// </summary>
		/// <param name="userId">
		/// Specifies the user context for the OrganizationService that is retrieved.
		/// A null value indicates the SYSTEM user.
		/// A Guid.Empty value indicates the same user a IExecutionContext.UserId
		/// Any other value indicates a specific user.
		/// </param>
		public IOrganizationService GetOrgService(Guid? userId)
		{
			IOrganizationServiceFactory factory = this.GetOrganizationServiceFactory();
			var service = factory.CreateOrganizationService(userId);

			return service;
		}

		/// <summary>
		/// Gets the specified shared variable from the context and optionally searches any parent contexts.
		/// </summary>
		/// <typeparam name="T">The type of the shared variable</typeparam>
		/// <param name="name">The name of the shared variable.</param>
		/// <param name="includeParentContexts">
		/// A value indicating that the parent context(s) should be included in the search.
		/// </param>
		/// <returns></returns>
		public T GetSharedVariable<T>(string name, bool includeParentContexts = true, T defaultValue = default(T))
		{
			T result = defaultValue;
			object sv = null;
			var ctx = this.ExecutionContext;

			do
			{
				if (ctx.SharedVariables.TryGetValue(name, out sv))
				{
					result = (sv is T) ? (T)sv : defaultValue;
					break;
				}

				ctx = this.GetParentContext(ctx);
			} while (ctx != null);

			return result;
		}

		/// <summary>
		/// Asserts the specified condition by throwing an InvalidPluginExecutionException if the condition is false.
		/// </summary>
		/// <param name="condition">The condition to assert.</param>
		/// <param name="message">The message for the exception.</param>
		/// <param name="args">Arguments used for substituion into the placeholders contained in the message.</param>
		public void Assert(bool condition, string message, params object[] args)
		{
			if (!condition)
			{
				throw new InvalidPluginExecutionException(
					"An assertion failed in the [{0}] plugin.\n".Substitute(this.ActionTypeName) +
					message.Substitute(args));
			}
		}

		/// <summary>
		/// Throws an InvalidPluginExecutionException with the specified message and inner exception.
		/// </summary>
		/// <param name="message">The message for the exception.</param>
		/// <param name="inner">The inner exception.</param>
		/// <param name="args">Arguments used for substitution into the placeholders contains in the message.</param>
		public void Throw(string message, Exception inner, params object[] args)
		{
			throw new InvalidPluginExecutionException(
				"An exception has occurred in the [{0}] plugin.\n".Substitute(this.ActionTypeName) +
				message.Substitute(args) +
				((inner != null) ? "\n" + inner.Message : string.Empty),
				inner);
		}

		/// <summary>
		/// Traces an Exception with the specified message.
		/// </summary>
		/// <param name="message">The message for the exception.</param>
		/// <param name="ex">The exception.</param>
		/// <param name="args">Arguments used for substitution into the placeholders contains in the message.</param>
		public void TraceException(string message, Exception ex, params object[] args)
		{
			this.Trace(
				"An exception has occurred in the [{0}].\n{1}\n{2}".Substitute(
				this.ActionTypeName,
				message.Substitute(args),
				ex.ToTraceString()));
		}

		/// <summary>
		/// Traces data to the tracing service if it is available.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="args"></param>
		public virtual void Trace(string message, params object[] args)
		{
			if (this.TracingService == null) { return; }
			this.TracingService.Trace(message, args);
		}

		#endregion

		#region Helper Methods

		public Exception BuildInnerException(Exception ex)
		{
			if (ex == null) { return null; }
			if (CanSendException(ex)) { return ex; }

			return new InvalidPluginExecutionException(String.Format("Exception of type {0} occurred but it could not be serialized and sent from the sandbox. \r\nException Data: {1}",
				ex.GetType(), ex.ToString()));
		}

		public bool CanSendException(Exception ex)
		{
			if (ex == null)
				return true;

			if (ExecutionContext.IsolationMode == (int)PluginAssemblyIsolationMode.None) { return true; }

			// Even though Exceptions are marked serializable, custom exceptions get serialized but 
			// cannot be deserialized when sent form the sandbox to the web service which causes the
			// user to get back a serialization exception instead of their real exception. 

			// Since we're ILMerging things, we can just add a check whether the assembly matches this type's assembly to
			// tell us whether this is a custom Exception type.
			if (typeof(IEventContext).Assembly == ex.GetType().Assembly) { return false; }

			return CanSendException(ex.InnerException);
		}

		#endregion
	}
}
