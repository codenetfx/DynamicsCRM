using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hsl.Xrm.Sdk.Plugin.Service
{
	public static class ResultSetResponseReader
	{
		private delegate void EntityReadAction(IDataReader r, Entity e);

		public static ResultSetResponse ReadRecords(IServicePluginContext ctx, IDataReader reader)
		{
			return ReadRecords(ctx.UserOrgService, ctx.ExecutionContext.PrimaryEntityName, reader);
		}

		public static ResultSetResponse ReadRecords(IOrganizationService svc, string entityName, IDataReader reader)
		{
			var req = new RetrieveEntityRequest
			{
				EntityFilters = EntityFilters.Attributes,
				LogicalName = entityName,
			};
			var resp = (RetrieveEntityResponse)svc.Execute(req);
			return ReadRecords(resp.EntityMetadata, reader);
		}

		public static ResultSetResponse ReadRecords(EntityMetadata metadata, IDataReader reader)
		{
			List<Entity> results = new List<Entity>();
			var columns = new Dictionary<string, bool>();
			for (var i = 0; i < reader.FieldCount; i++)
			{
				columns[reader.GetName(i).ToLower()] = false;
			}
			var attrReaders = metadata.Attributes.Select(a => GetReaderMethod(a, columns)).Where(x => x != null);
			var unusedColumnReaders = columns.Where(x => !x.Value).Select((kvp) =>
			{
				var aln = kvp.Key;
				return (EntityReadAction)((r, e) =>
				{
					e[aln] = ValueOrNull(r, aln);
				});
			});

			var columnReaders = attrReaders.Concat(unusedColumnReaders).ToList();

			while (reader.Read())
			{
				var entity = new Entity(metadata.LogicalName);
				foreach (var readAction in columnReaders)
				{
					readAction(reader, entity);
				}
				results.Add(entity);
			}

			return new ResultSetResponse(results);
		}

		/// <summary>
		/// Returns a method that sets the values on the entity based on the reader columns.
		/// </summary>
		private static EntityReadAction GetReaderMethod(AttributeMetadata attr, Dictionary<string, bool> columns)
		{
			var isDate = attr is DateTimeAttributeMetadata;
			var sqlColumnName = attr.LogicalName;

			// For dates, default to using the UTC column if it is there. The results should be utc either way to non-filtered views don't have 'utc' at the end.
			if (isDate && columns.ContainsKey(attr.LogicalName + "utc"))
			{
				sqlColumnName = attr.LogicalName + "utc";
				if (columns.ContainsKey(attr.LogicalName))
				{
					columns[attr.LogicalName] = true;
				}
			}

			if (!attr.IsValidForRead.Value || attr.AttributeType.Value == AttributeTypeCode.Virtual || attr.AttributeOf != null || !columns.ContainsKey(sqlColumnName))
			{
				return null;
			}



			columns[sqlColumnName] = true;

			if (attr is BooleanAttributeMetadata)
			{
				return GetConvertAction(sqlColumnName, typeof(bool));
			}
			if (attr is DateTimeAttributeMetadata)
			{
				return GetConvertAction(sqlColumnName, typeof(DateTime));
			}
			if (attr is DecimalAttributeMetadata)
			{
				return GetConvertAction(sqlColumnName, typeof(decimal));
			}
			if (attr is DoubleAttributeMetadata)
			{
				return GetConvertAction(sqlColumnName, typeof(double));
			}
			if (attr is IntegerAttributeMetadata)
			{
				return GetConvertAction(sqlColumnName, typeof(int));
			}
			if (attr is MoneyAttributeMetadata)
			{
				return (r, e) =>
				{
					var value = ValueOrNull(r, sqlColumnName);
					if (value == null)
					{
						e[sqlColumnName] = null;
					}
					else
					{
						e[sqlColumnName] = new Money(Convert.ToDecimal(value));
					}
				};
			}
			if (attr is LookupAttributeMetadata)
			{
				var targets = ((LookupAttributeMetadata)attr).Targets;
				if (targets.Length != 1)
				{
					throw new InvalidPluginExecutionException("Unable to read {0}. Currently only works for lookups with a single target.");
				}
				return (r, e) =>
				{
					var id = ValueOrNull(r, sqlColumnName);
					if (id == null)
					{
						e[sqlColumnName] = null;
					}
					else
					{
						var nameColumn = sqlColumnName + "name";
						if (!columns.ContainsKey(nameColumn))
						{
							throw new InvalidPluginExecutionException("Expected column with name " + nameColumn);
						}
						columns[nameColumn] = true;
						var name = ValueOrNull(r, nameColumn);
						e[sqlColumnName] = new EntityReference
						{
							Id = (Guid)Convert.ChangeType(id, typeof(Guid)),
							LogicalName = targets.Single(),
							Name = Convert.ToString(name),
						};
					}
				};
			}
			if (attr is MemoAttributeMetadata || attr is StringAttributeMetadata)
			{
				return GetConvertAction(sqlColumnName, typeof(string));
			}
			if (attr is EnumAttributeMetadata)
			{
				return (r, e) =>
				{
					var value = ValueOrNull(r, sqlColumnName);
					if (value == null)
					{
						e[sqlColumnName] = null;
					}
					else
					{
						e[sqlColumnName] = new OptionSetValue(Convert.ToInt32(value));
					}
				};
			}

			// BigInts and others
			return (r, e) =>
			{
				e[sqlColumnName] = ValueOrNull(r, sqlColumnName);
			};
		}

		private static EntityReadAction GetConvertAction(string attr, Type type)
		{
			return (r, e) =>
			{
				var v = ValueOrNull(r, attr);
				try
				{
					if (v == null)
					{
						e[attr] = null;
					}
					else
					{
						e[attr] = Convert.ChangeType(v, type);
					}
				}
				catch (Exception ex)
				{
					throw new InvalidPluginExecutionException(String.Format("Failed conversion for attribute {0} from type {1} to type {2}",
						attr, v == null ? "<NULL>" : v.GetType().FullName, type.FullName) + "\r\n" + ex.Message, ex);
				}
			};
		}

		private static object ValueOrNull(IDataRecord reader, string column)
		{
			if (reader[column] is DBNull)
			{
				return null;
			}
			return reader[column];
		}
	}
}
