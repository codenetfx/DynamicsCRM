using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Xml.Linq;


namespace Hsl.Xrm.Sdk
{
    public static class OrganizationServiceExtensions
    {
        #region IOrganizationService Extensions Except RetrieveAll

        /// <summary>
        /// Retrieves a list of multiple DynamicEntities using the specified FilterExpression.
        /// </summary>
        /// <returns>A list of DynamicEntities.</returns>
        public static EntityCollection RetrieveMultiple(this IOrganizationService service, string entityName, ColumnSet columns, FilterExpression filter)
        {
            QueryExpression query = new QueryExpression(entityName);
            query.ColumnSet = columns;
            query.Criteria = filter;

            // Convert the results and return a list of DynamicEntities
            return service.RetrieveMultiple(query);
        }

        /// <summary>
        /// Retrieves a list of multiple DynamicEntities using the specified query from the ICrmService.
        /// </summary>
        /// <returns>A list of DynamicEntities.</returns>
        public static EntityCollection RetrieveMultiple(this IOrganizationService service, string entityName, ColumnSet columns, ConditionExpression[] conditions, LogicalOperator filterOp)
        {
            // Build the query
            FilterExpression filter = new FilterExpression();
            filter.FilterOperator = filterOp;
            filter.Conditions.AddRange(conditions);

            // Convert the results and return a list of DynamicEntities
            return service.RetrieveMultiple(entityName, columns, filter);
        }

        /// <summary>
        /// Retrieves a list of multiple DynamicEntities using the specified query from the ICrmService.
        /// </summary>
        /// <param name="service">The ICrmService to be used for retrieval of the records.</param>
        /// <param name="entityName"></param>
        /// <param name="columns"></param>
        /// <param name="condition"></param>
        /// <returns>A list of DynamicEntities.</returns>
        public static EntityCollection RetrieveMultiple(this IOrganizationService service, string entityName, ColumnSet columns, ConditionExpression condition)
        {
            return service.RetrieveMultiple(entityName, columns, new[] { condition }, LogicalOperator.And);
        }

        /// <summary>
        /// Retrieves a list of multiple DynamicEntities given the conditions.
        /// </summary>
        /// <returns>A list of DynamicEntities.</returns>
        public static EntityCollection RetrieveMultiple(this IOrganizationService service, string entityName, ColumnSet columns, string conditionAttribute, ConditionOperator conditionOp, object conditionValue)
        {
            // Build the query
            ConditionExpression condition =
                (conditionValue is Array) ?
                new ConditionExpression(conditionAttribute, conditionOp, (Array)conditionValue) :
                new ConditionExpression(conditionAttribute, conditionOp, conditionValue);

            return service.RetrieveMultiple(entityName, columns, condition);
        }

        #endregion

        #region IOrganizationService Extensions RetrieveAll

        /// <summary>
        /// Pages through records until options.MaxResults is reached. 
        /// </summary>
        public static IEnumerable<Entity> RetrieveAll(this IOrganizationService svc, QueryExpression query, RetrieveAllOptions options = null)
        {
            return RetrieveAll<Entity>(svc, query, options);
        }

        /// <summary>
        /// Pages through records until options.MaxResults is reached. 
        /// </summary>
        public static IEnumerable<T> RetrieveAll<T>(this IOrganizationService svc, QueryExpression query, RetrieveAllOptions options = null) where T : Entity
        {
            options = options ?? new RetrieveAllOptions();

            query.PageInfo = new PagingInfo { PageNumber = 1 };
            if (options.PageSize != 0)
            {
                query.PageInfo.Count = options.PageSize;
            }
            long resultsReturned = 0;
            while (true)
            {
                var resp = svc.RetrieveMultiple(query);

                foreach (var entity in resp.Entities)
                {
                    yield return (T)entity;
                    resultsReturned++;
                    if (resultsReturned >= options.MaxResults)
                    {
                        if (options.ErrorAtMaxResults)
                        {
                            throw new QueryLimitException(
                                "The number of results for the query exceeds the maximum number of results. ");
                        }
                        else
                        {
                            yield break;
                        }
                    }
                }

                if (!resp.MoreRecords)
                    break;
                query.PageInfo.PageNumber++;
                query.PageInfo.PagingCookie = resp.PagingCookie;
            }
        }

        /// <summary>
        /// Pages through records until options.MaxResults is reached. 
        /// </summary>
        public static IEnumerable<Entity> RetrieveAll(this IOrganizationService svc, string entityName,
            ColumnSet columns, FilterExpression filter, RetrieveAllOptions options = null)
        {
            var query = new QueryExpression(entityName);
            query.ColumnSet = columns;
            query.Criteria = filter;
            return svc.RetrieveAll(query, options);
        }

        /// <summary>
        /// Pages through records until options.MaxResults is reached. 
        /// </summary>
        public static IEnumerable<Entity> RetrieveAll(this IOrganizationService service, string entityName,
            ColumnSet columns, ConditionExpression condition, RetrieveAllOptions options = null)
        {
            return service.RetrieveAll(entityName, columns, new[] { condition }, LogicalOperator.And, options);
        }


        /// <summary>
        /// Pages through records until options.MaxResults is reached. 
        /// </summary>
        public static IEnumerable<Entity> RetrieveAll(this IOrganizationService svc, string entityName, ColumnSet columns,
            IEnumerable<ConditionExpression> conditions, LogicalOperator logicalOperator, RetrieveAllOptions options = null)
        {
            var filter = new FilterExpression { FilterOperator = logicalOperator };
            foreach (var condition in conditions)
            {
                filter.AddCondition(condition);
            }

            return svc.RetrieveAll(entityName, columns, filter, options);
        }

		public static IEnumerable<Entity> RetrieveAll(this IOrganizationService svc, string fetchXml, RetrieveAllOptions options = null)
		{
			options = options ?? new RetrieveAllOptions();

			var fetchDoc = XElement.Parse(fetchXml);

			if (options.PageSize != 0)
				fetchDoc.SetAttributeValue("count", options.PageSize.ToString());

			var pageNumber = 1;

			var resultsReturned = 0;
			while (true)
			{
				var resp = svc.RetrieveMultiple(new FetchExpression(fetchDoc.ToString()));

				foreach (var entity in resp.Entities)
				{
					yield return entity;
					resultsReturned++;
					if (resultsReturned >= options.MaxResults)
					{
						if (options.ErrorAtMaxResults)
						{
							throw new QueryLimitException(
								"The number of results for the query exceeds the maximum number of results. ");
						}
						else
						{
							yield break;
						}
					}
				}

				if (!resp.MoreRecords)
					break;

				pageNumber++;
				fetchDoc.SetAttributeValue("page", pageNumber.ToString());
				fetchDoc.SetAttributeValue("paging-cookie", resp.PagingCookie);
			}
		}

		public static IEnumerable<Entity> RetrieveAll(this IOrganizationService svc, FetchExpression query, RetrieveAllOptions options = null)
		{
			return svc.RetrieveAll(query.Query, options);
		} 

        #endregion
    }
}
