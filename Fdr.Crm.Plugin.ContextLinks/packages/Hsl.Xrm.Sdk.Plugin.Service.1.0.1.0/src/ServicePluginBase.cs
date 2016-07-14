using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Hsl.Xrm.Sdk.Plugin.Service
{
	public abstract class ServicePluginBase : PluginBase, IPlugin
	{
		public const string ServiceFieldName_ActionInputName = "Name";
		public const string ServiceFieldName_ActionInputData = "InData";
		public const string ServiceFieldName_ActionOutputData = "OutData";

		public const string ServiceFieldName_Data = "data";
		public const string ServiceFieldName_IsComplete = "iscomplete";
		public const string ServiceFieldName_Progress = "progress";

		#region Constructors/Destructors

		public ServicePluginBase() : base() { }
		public ServicePluginBase(string unsecureConfig, string secureConfig) : base(unsecureConfig, secureConfig) { }

		#endregion

		#region Properties

		/// <summary>
		/// Gets the context information for the service plugin.
		/// </summary>
		public IServicePluginContext ServiceContext { get; private set; }			

		#endregion

		public abstract object ExecuteService(string data);

		/// <summary>
		/// Updates the completion status of the service plugin to allow the async processing to be skipped.
		/// </summary>
		public void MarkCompleteAndSkipAsyncProcessing()
		{
			ServiceContext.ServiceDataInfo.Target[GetPrefixedFieldName(ServicePluginBase.ServiceFieldName_IsComplete)] = true;
		}

		/// <summary>
		/// Updates the progress of the service plugin by setting the entity data associated with the request.
		/// </summary>
		/// <param name="progress"></param>
		public void UpdateProgress(object progress)
		{
			var ctx = ServiceContext;
			if ((ctx.OperationType != ServicePluginOperationType.Create) || ctx.IsExecutingSynchronously) { return; }

			var targetData = ctx.ServiceDataInfo.Target;
			var update = new Entity(targetData.LogicalName);
			update.Id = targetData.Id;
			update[GetPrefixedFieldName(ServicePluginBase.ServiceFieldName_Progress)] = progress;
			ctx.SystemOrgService.Update(update);
		}

		#region PluginBase Overrides 

		/// <summary>
		/// Gets the data and executes the service plugin action/logic.
		/// </summary>
		public sealed override void ExecuteAction()
		{
			var ctx = Context as ServicePluginContext;
			if (ctx == null) { return; }
			ServiceContext = ctx;

			ctx.ServiceDataInfo = GetDataInfo();
			if (ctx.ServiceDataInfo == null) { return; }

			var resultData = ExecuteService(ctx.ServiceDataInfo.Data);
			SetResultData(resultData);

		}

		/// <summary>
		/// Create the context for the service plugin.
		/// </summary>
		/// <returns></returns>
		protected override IPluginEventContext CreateContext()
		{
			return new ServicePluginContext();
		}

		#endregion

		#region Helpers

		/// <summary>
		/// Gets a field name with the publisher prefix prepended to it.
		/// </summary>
		public string GetPrefixedFieldName(string fieldName)
		{
			return GetCustomizationPrefix(Context.ExecutionContext.PrimaryEntityName) + "_" + fieldName;
		}

		/// <summary>
		/// Processes the request and gets the data needed to execute the service plugin based upon the operation type of the plugin.
		/// </summary>
		private ServicePluginDataInfo GetDataInfo()
		{
			var ctx = ServiceContext;
			switch (ctx.OperationType)
			{
				case ServicePluginOperationType.Create:
					return GetDataInfoFromEntity(ctx.TargetInput);

				case ServicePluginOperationType.RetrieveMultiple:
					return GetDataInfoFromQuery();

				case ServicePluginOperationType.Action:
				default:
					return GetDataInfoFromCustomAction();
			}
		}

		/// <summary>
		/// Gets the data for a custom action service plugin.
		/// </summary>
		private ServicePluginDataInfo GetDataInfoFromCustomAction()
		{
			var input = Context.ExecutionContext.InputParameters;
			var inData = input.GetItemAs<string>(ServicePluginBase.ServiceFieldName_ActionInputData);
			var name = input.GetItemAs<string>(ServicePluginBase.ServiceFieldName_ActionInputName);

			return new ServicePluginDataInfo { Name = name, Data = inData, };
		}

		/// <summary>
		/// Gets the data for a retrieve multiple service plugin.
		/// </summary>
		private ServicePluginDataInfo GetDataInfoFromQuery()
		{
			var ctx = ServiceContext;
			List<ConditionExpression> conditions;
			var query = ctx.ExecutionContext.InputParameters.GetItemAs<QueryExpression>("Query");
			if (query == null)
			{
				var fetch = ctx.ExecutionContext.InputParameters.GetItemAs<FetchExpression>("Query");
				if (fetch == null) { return null; }

				var xDoc = XDocument.Parse(fetch.Query);
				conditions =
					(
					from c in xDoc.Descendants("condition")
					where c.Attribute("value") != null
					select
						new ConditionExpression(c.Attribute("attribute").Value,
							ParseLikeOperator(c.Attribute("operator").Value),
							c.Attribute("value").Value)
					).ToList();
			}
			else
			{
				if (query.Criteria == null) { return null; }

				conditions = (query.Criteria.Filters ?? Enumerable.Empty<FilterExpression>())
					.SelectMany(x => x.Conditions)
					.Concat(query.Criteria.Conditions ?? Enumerable.Empty<ConditionExpression>()).ToList();
			}

			if (!conditions.Any()) { return null; }

			var e = ConditionsToEntity(conditions);
			return GetDataInfoFromEntity(e);
		}

		/// <summary>
		/// Gets the data for a service plugin based on entity data, i.e. retrieve multiple and create.
		/// </summary>
		private ServicePluginDataInfo GetDataInfoFromEntity(Entity entity)
		{
			var attributes = entity.Attributes.Where(x => !String.IsNullOrEmpty(x.Value as string));
			var dataAttr = attributes.FirstOrDefault(x => !x.Key.EndsWith("_name"));
			var nameAttr = attributes.FirstOrDefault(x => x.Key.EndsWith("_name"));

			if (nameAttr.Value == null && dataAttr.Value == null)
				return null;

			return new ServicePluginDataInfo
			{
				Name = (string)nameAttr.Value,
				Data = (string)dataAttr.Value,
				DataAttribute = dataAttr.Key,
				Target = entity,
			};
		}

		/// <summary>
		/// Transforms the data in conditions supplied to the service plugin into and entity.  This facilitates the processing of create and retrieve multiple plugin
		/// data so that they can be treated similarly.
		/// </summary>
		private Entity ConditionsToEntity(IEnumerable<ConditionExpression> conditions)
		{
			var entity = new Entity();
			foreach (var c in conditions)
			{
				string value = c.Values.FirstOrDefault() as string;
				if (string.IsNullOrEmpty(value)) { continue; }

				if (c.Operator == ConditionOperator.Like)
				{
					value = value.StartsWith("%") ? value.Substring(1) : value;
					value = value.EndsWith("%") ? value.Substring(0, value.Length - 1) : value;
				}

				entity[c.AttributeName] = value;
			}
			return entity;
		}

		/// <summary>
		/// Parses the conditional data passed to a service plugin via a query.
		/// </summary>
		private static ConditionOperator ParseLikeOperator(string op)
		{
			// The only operator with special meaning is like because we strip of the leading/trailing % sign. 
			// Otherwise it doesn't matter.
			return string.Equals(op, "like", StringComparison.InvariantCultureIgnoreCase)
				? ConditionOperator.Like
				: ConditionOperator.Equal;
		}

		/// <summary>
		/// Gets the publisher prefix based on the supplied entity name.
		/// </summary>
		private static string GetCustomizationPrefix(string entityName)
		{
			var ix = entityName.IndexOf('_');
			return entityName.Substring(0, ix);
		}

		/// <summary>
		/// Sets the result data based upon the operation type of the plugin.
		/// </summary>
		private void SetResultData(object data)
		{
			switch (ServiceContext.OperationType)
			{
				case ServicePluginOperationType.Create:
					SetResultDataForCreate(data);
					break;

				case ServicePluginOperationType.RetrieveMultiple:
					SetResultDataForRetrieveMultiple(data);
					break;

				case ServicePluginOperationType.Action:
				default:
					SetResultDataForCustomAction(data);
					break;
			}
		}

		/// <summary>
		/// Sets the result data for a custom action service plugin.
		/// </summary>
		/// <param name="data"></param>
		private void SetResultDataForCustomAction(object data)
		{
			if (data == null || data is string)
			{
				Context.ExecutionContext.OutputParameters[ServiceFieldName_ActionOutputData] = data;
			}
			else
			{
				Context.Throw("Unknown return type: {0}", null, data.GetType());
			}
		}

		/// <summary>
		/// Sets the result data for a create service plugin.
		/// </summary>
		/// <param name="data"></param>
		private void SetResultDataForCreate(object data)
		{
			var ctx = ServiceContext;
			var dataInfo = ctx.ServiceDataInfo;

			// Get attribute names
			var dataAttr = dataInfo.DataAttribute ?? GetPrefixedFieldName(ServiceFieldName_Data);
			var isCompleteAttr = GetPrefixedFieldName(ServiceFieldName_IsComplete);

			// Determine if processing is complete and update result data accordingly
			bool isComplete = true;
			object resultData;
			if (data is ProcessingNotCompleteResult)
			{
				if (ctx.IsExecutingAsynchronously) { return; }
				resultData = ((ProcessingNotCompleteResult)data).Data;
				isComplete = false;
			}
			else
			{
				resultData = data;
			}
			if (resultData == null) { return; }

			// If we have 
			if (resultData is string)
			{
				var targetInput = ctx.TargetInput;
				var record = ctx.IsExecutingAsynchronously ? new Entity(targetInput.LogicalName) { Id = targetInput.Id } : targetInput;
				record[dataAttr] = resultData;
				record[isCompleteAttr] = isComplete;

				if (ctx.IsExecutingAsynchronously)
				{
					ctx.SystemOrgService.Update(record);
				}
				else if (ctx.PluginContext.Stage != (int)SdkMessageProcessingStepStage.PreOperation)
				{
					ctx.Throw("Invalid Plugin Registration: Service plugins must be registered in the pre-operation stage when executing synchronously using the 'Create' method.", null);
				}
			}
			else
			{
				ctx.Throw("Unknown return type: {0}", null, resultData.GetType());
			}
		}

		/// <summary>
		/// Sets the result data for a retrieve multiple service plugin.
		/// </summary>
		/// <param name="resultData"></param>
		private void SetResultDataForRetrieveMultiple(object resultData)
		{
			var ctx = ServiceContext;

			var output = (EntityCollection)ctx.ExecutionContext.OutputParameters["BusinessEntityCollection"];
			output.Entities.Clear();

			if (resultData == null || resultData is string)
			{
				var dataInfo = ctx.ServiceDataInfo;
				var outEntity = new Entity(ctx.ExecutionContext.PrimaryEntityName);
				outEntity[dataInfo.DataAttribute ?? GetPrefixedFieldName(ServiceFieldName_Data)] = resultData;
				output.Entities.Add(outEntity);
			}
			else if (resultData is ResultSetResponse)
			{
				var results = ((ResultSetResponse)resultData);
				output.Entities.AddRange(results.Entities);
				output.EntityName = results.EntityName;
				output.TotalRecordCount = results.TotalRecordCount ?? output.Entities.Count;
				output.MoreRecords = results.MoreRecords ?? false;
				output.TotalRecordCountLimitExceeded = results.TotalRecordCountLimitExceeded ?? false;
				output.PagingCookie = results.PagingCookie;
			}
			else
			{
				ctx.Throw("Unknown return type: {0}", null, resultData.GetType());
			}
		}

		#endregion

	}
}
