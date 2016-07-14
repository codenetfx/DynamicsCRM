using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hsl.Xrm.Sdk.Plugin.Service
{
	public class ResultSetResponse
	{

		public ResultSetResponse()
		{
		}
		public ResultSetResponse(IEnumerable<Entity> entities)
		{
			Entities = entities;
		}
		public ResultSetResponse(EntityCollection entities)
		{
			Entities = entities.Entities;
			TotalRecordCount = entities.TotalRecordCount;
			TotalRecordCountLimitExceeded = entities.TotalRecordCountLimitExceeded;
			MoreRecords = entities.MoreRecords;
			PagingCookie = entities.PagingCookie;
			EntityName = entities.EntityName;
		}

		public IEnumerable<Entity> Entities { get; set; }
		public string EntityName { get; set; }
		public int? TotalRecordCount { get; set; }
		public bool? TotalRecordCountLimitExceeded { get; set; }
		public bool? MoreRecords { get; set; }
		public string PagingCookie { get; set; }

	}
}
