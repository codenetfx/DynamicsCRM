using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;


namespace Hsl.Xrm.Sdk
{
    public static class XrmExtensions
    {
        #region Decimal Extensions

        /// <summary>
        /// Transforms the decimal value into a Money object.
        /// </summary>
        /// <param name="target">The target decimal value to be transformed into a Money object.</param>
        /// <returns>A Money object whose value will be set to the value of the target decimal.</returns>
        public static Money ToMoney(this decimal target)
        {
            return new Money(target);
        }

        /// <summary>
        /// Transforms the decimal value into a Money object.
        /// </summary>
        /// <param name="target">The target decimal value to be transformed into a Money object.</param>
        /// <returns>
        /// A Money object whose value will be set to the value of the target decimal, or null if the decimal value is null.
        /// </returns>
        public static Money ToMoney(this decimal? target)
        {
            return target.HasValue ? new Money(target.Value) : null;
        }

        #endregion

        #region Entity Extensions

        #region General Helper Methods

        /// <summary>
        /// Create a new entity as a copy of the baseEntity and copies the attributes from the mergeEntity into the copy of the baseEntity.
        /// </summary>
        /// <param name="baseEntity">The base entity that is initially copied as part of the merge.</param>
        /// <param name="mergedEntity">The entity whose attributes are merged into the copy of the base entity.</param>
        /// <returns></returns>
        public static Entity Merge(this Entity baseEntity, Entity mergedEntity)
        {
            Entity result = null;

            if ((baseEntity == null) || (mergedEntity == null)) { throw new Exception("A baseEntity and mergedEntity value must be specified when merging entities."); }
            if (baseEntity.LogicalName != mergedEntity.LogicalName) { throw new InvalidOperationException("The entities must be of the same logical type to be merged."); }

            result = baseEntity.Copy();

            // Copy the attributes and formatted values of the image to the result
            foreach (var f in mergedEntity.Attributes)
            {
                result[f.Key] = mergedEntity.CopyAttribute(f.Key);
            }

            foreach (var f in mergedEntity.FormattedValues)
            {
                result.FormattedValues[f.Key] = (f.Value != null) ? string.Copy(f.Value) : null;
            }


            return result;
        }

        /// <summary>
        /// Creates a deep copy of the specified attributes on the target entity.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static object CopyAttribute(this Entity target, string attribute)
        {
            object result = null;

            object fieldValue = target.GetFieldValue(attribute);
            if (fieldValue != null)
            {

                switch (fieldValue.GetType().Name)
                {
                    case "BooleanManagedProperty":
                        // We are ignoring these attributes
                        break;

                    case "EntityCollection":
                        var ec = (EntityCollection)fieldValue;
                        result = new EntityCollection()
                        {
                            EntityName = (ec.EntityName != null) ? string.Copy(ec.EntityName) : null,
                            MinActiveRowVersion = (ec.MinActiveRowVersion != null) ? string.Copy(ec.MinActiveRowVersion) : null,
                            MoreRecords = ec.MoreRecords,
                            PagingCookie = (ec.PagingCookie != null) ? string.Copy(ec.PagingCookie) : null,
                            TotalRecordCount = ec.TotalRecordCount,
                            TotalRecordCountLimitExceeded = ec.TotalRecordCountLimitExceeded
                        };

                        foreach (var e in ec.Entities)
                        {
                            ((EntityCollection)result).Entities.Add(e.Copy());
                        }
                        break;

                    case "EntityReference":
                        var er = (EntityReference)fieldValue;
                        result = new EntityReference()
                        {
                            Id = er.Id,
                            LogicalName = (er.LogicalName != null) ? string.Copy(er.LogicalName) : null,
                            Name = (er.Name != null) ? string.Copy(er.Name) : null
                        };
                        break;

                    case "Money":
                        var m = (Money)fieldValue;
                        result = new Money()
                        {
                            Value = m.Value
                        };
                        break;

                    case "OptionSetValue":
                        var osv = (OptionSetValue)fieldValue;
                        result = new OptionSetValue()
                        {
                            Value = osv.Value
                        };
                        break;

                    case "String":
                        result = string.Copy((string)fieldValue);
                        break;

                    default:
                        // The remainder of the attributes should be value types so this should be ok
                        result = fieldValue;
                        break;

                }
            }

            return result;
        }

        /// <summary>
        /// Generates a deep copy of the target entity.  This includes only the attributes and formatted attributes.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static Entity Copy(this Entity target)
        {
            Entity result = null;

            if (target != null)
            {
                result = new Entity()
                {
                    Id = target.Id,
                    LogicalName = (target.LogicalName != null) ? string.Copy(target.LogicalName) : null
                };

                foreach (var a in target.Attributes)
                {
                    result[a.Key] = target.CopyAttribute(a.Key);
                }

                foreach (var f in target.FormattedValues)
                {
                    result.FormattedValues[f.Key] = (f.Value != null) ? string.Copy(f.Value) : null;
                }
            }

            return result;
        }

        #endregion

        #region Generic Field Value Retrieval Methods

        public static string GetFormattedValueOrDefault(this Entity target, string attribute)
        {
            return target.GetFormattedValueOrDefault(attribute, default(string));
        }

        public static string GetFormattedValueOrDefault(this Entity target, string attribute, string defaultValue)
        {
            string result = defaultValue;

            if (target != null)
            {
                string formattedValue;
                if (target.FormattedValues.TryGetValue(attribute, out formattedValue))
                {
                    result = formattedValue;
                }
            }

            return result;
        }

        public static string GetFieldValueAsString(this Entity target, string attribute, bool useDisplayedValue = false)
        {
            string result = string.Empty;
            object val = target.GetFieldValue(attribute);
            val = (val is AliasedValue) ? ((AliasedValue)val).GetValue() : val;
            if (val != null)
            {
                switch (val.GetType().Name)
                {
                    case "Int64":
                    case "Decimal":
                    case "Double":
                    case "Int32":
                        result = useDisplayedValue && target.FormattedValues.Contains(attribute)
                               ? target.GetFormattedValueOrDefault(attribute)
                               : val.ToString();
                        break;

                    case "Boolean":
                        result = useDisplayedValue
                               ? target.GetFormattedValueOrDefault(attribute)
                               : val.ToString();
                        break;

                    case "EntityReference":
                        result = useDisplayedValue
                               ? ((EntityReference)val).GetNameOrDefault(string.Empty)
                               : ((EntityReference)val).GetIdOrDefault().ToString("B");
                        break;

                    case "DateTime":
                        result = useDisplayedValue
                               ? target.GetFormattedValueOrDefault(attribute, string.Empty)
                               : ((DateTime)val).ToString("yyyy-MM-ddTHH:mm:ssZ");
                        break;

                    case "String":
                        result = (string)val ?? string.Empty;
                        break;

                    case "Money":
                        result = useDisplayedValue
                               ? target.GetFormattedValueOrDefault(attribute)
                               : ((Money)val).Value.ToString();
                        break;

                    case "OptionSetValue":
                        result = useDisplayedValue
                               ? target.GetFormattedValueOrDefault(attribute, string.Empty)
                               : ((OptionSetValue)val).Value.ToString();
                        break;

                    case "Guid":
                        result = ((Guid)val).ToString("B");
                        break;

                    default:
                        break;

                }
            }

            return result;
        }

        public static object GetFieldValue(this Entity target, string attribute)
        {
            object result = null;

            if (target != null)
            {
                object fieldValue;
                if (target.Attributes.TryGetValue(attribute, out fieldValue))
                {
                    result = fieldValue;
                }
            }

            return result;
        }

        public static T GetFieldValueOrDefault<T>(this Entity target, string attribute)
        {
            return target.GetFieldValueOrDefault<T>(attribute, default(T));
        }

        public static T GetFieldValueOrDefault<T>(this Entity target, string attribute, T defaultValue)
        {
            if (target == null) { return defaultValue; }

            // Return the default value if the field isn't found
            object fieldValue;
            if (!target.Attributes.TryGetValue(attribute, out fieldValue)) { return defaultValue; }

            // Return the aliased value if the types match
            var resultIsAliased = fieldValue is AliasedValue;
            var defaultIsAliased = typeof(T) == typeof(AliasedValue);
            if (defaultIsAliased && resultIsAliased) { return (T)fieldValue; }

            // Unwrap aliased value if necessary and return the value if the types match
            fieldValue = resultIsAliased ? ((AliasedValue)fieldValue).GetValue() : fieldValue;
            if (fieldValue is T) { return (T)fieldValue; }
            if (fieldValue != null) { throw new InvalidOperationException("The specified field type does not match the actual field type."); }

            return defaultValue;
        }

        public static bool ContainsNonNullValue(this Entity target, string attribute)
        {
            return target.Contains(attribute) && target[attribute] != null;
        }

        #endregion

        #region EntityReference Field Value Retrieval Methods

        public static Guid? GetLookupId(this Entity target, string attribute)
        {
            Guid? result = null;

            if (target != null)
            {
                EntityReference field = target.GetAttributeValue<EntityReference>(attribute);
                if (field != null)
                {
                    result = field.Id;
                }
            }

            return result;
        }

        public static Guid GetLookupIdOrDefault(this Entity target, string attribute)
        {
            return target.GetLookupIdOrDefault(attribute, default(Guid));
        }

        public static Guid GetLookupIdOrDefault(this Entity target, string attribute, Guid defaultValue)
        {
            Guid? result = target.GetLookupId(attribute);
            return result ?? defaultValue;
        }

        public static string GetLookupName(this Entity target, string attribute)
        {
            string result = null;

            if (target != null)
            {
                EntityReference field = target.GetAttributeValue<EntityReference>(attribute);
                if (field != null)
                {
                    result = field.Name;
                }
            }

            return result;
        }

        public static string GetLookupNameOrDefault(this Entity target, string attribute)
        {
            return target.GetLookupNameOrDefault(attribute, default(string));
        }

        public static string GetLookupNameOrDefault(this Entity target, string attribute, string defaultValue)
        {
            string result = target.GetLookupName(attribute);
            return result ?? defaultValue;
        }

        public static string GetLookupLogicalName(this Entity target, string attribute)
        {
            string result = null;

            if (target != null)
            {
                EntityReference field = target.GetAttributeValue<EntityReference>(attribute);
                if (field != null)
                {
                    result = field.LogicalName;
                }
            }

            return result;
        }

        public static string GetLookupLogicalNameOrDefault(this Entity target, string attribute)
        {
            return target.GetLookupLogicalNameOrDefault(attribute, default(string));
        }

        public static string GetLookupLogicalNameOrDefault(this Entity target, string attribute, string defaultValue)
        {
            string result = target.GetLookupLogicalName(attribute);
            return result ?? defaultValue;
        }

        #endregion

        #region Money Field Value Retrieval Methods

        public static decimal? GetMoneyValue(this Entity target, string attribute)
        {
            decimal? result = null;

            if (target != null)
            {
                Money field = target.GetAttributeValue<Money>(attribute);
                if (field != null)
                {
                    result = field.Value;
                }
            }

            return result;
        }

        public static decimal GetMoneyValueOrDefault(this Entity target, string attribute)
        {
            return target.GetMoneyValueOrDefault(attribute, default(decimal));
        }

        public static decimal GetMoneyValueOrDefault(this Entity target, string attribute, decimal defaultValue)
        {
            decimal? result = target.GetMoneyValue(attribute);
            return result ?? defaultValue;
        }

        #endregion

        #region OptionSetValue Value Retrieval Methods

        public static int? GetPicklistValue(this Entity target, string attribute)
        {
            int? result = null;

            if (target != null)
            {
                OptionSetValue field = target.GetAttributeValue<OptionSetValue>(attribute);
                if (field != null)
                {
                    result = field.Value;
                }
            }

            return result;
        }

        public static int GetPicklistValueOrDefault(this Entity target, string attribute)
        {
            return target.GetPicklistValueOrDefault(attribute, default(int));
        }

        public static int GetPicklistValueOrDefault(this Entity target, string attribute, int defaultValue)
        {
            int? result = target.GetPicklistValue(attribute);
            return result ?? defaultValue;
        }

        public static TEnum GetPicklistValueOrDefault<TEnum>(this Entity target, string attribute, TEnum defaultValue) where TEnum : struct
        {
            TEnum result;
            var value = target.GetPicklistValue(attribute);

            if (!Enum.TryParse(value.HasValue ? value.ToString() : string.Empty, true, out result))
            {
                result = defaultValue;
            }

            return result;

        }
        
        #endregion

        #endregion

        #region EntityReference Extensions

        #region General Helper Methods

        public static Entity ToEntity(this EntityReference target)
        {
            return (target != null) ? new Entity(target.LogicalName) { Id = target.Id } : null;
        }

        #endregion

        #region Id Property Retrieval Methods

        public static Guid? GetId(this EntityReference target)
        {
            return (target != null) ? (Guid?)target.Id : null;
        }

        public static Guid GetIdOrDefault(this EntityReference target)
        {
            return target.GetIdOrDefault(default(Guid));
        }

        public static Guid GetIdOrDefault(this EntityReference target, Guid defaultValue)
        {
            return target.GetId() ?? defaultValue;
        }

        #endregion

        #region Name Property Retrieval Methods

        public static string GetName(this EntityReference target)
        {
            return (target != null) ? target.Name : null;
        }

        public static string GetNameOrDefault(this EntityReference target)
        {
            return target.GetNameOrDefault(default(string));
        }

        public static string GetNameOrDefault(this EntityReference target, string defaultValue)
        {
            return target.GetName() ?? defaultValue;
        }

        #endregion

        #region LogicalName Property Retrieval Methods

        public static string GetLogicalName(this EntityReference target)
        {
            return (target != null) ? target.LogicalName : null;
        }

        public static string GetLogicalNameOrDefault(this EntityReference target)
        {
            return target.GetLogicalNameOrDefault(default(string));
        }

        public static string GetLogicalNameOrDefault(this EntityReference target, string defaultValue)
        {
            return target.GetLogicalName() ?? defaultValue;
        }

        #endregion

        #endregion

        #region Enum Extensions

        public static OptionSetValue ToOptionSetValue(this Enum enumValue)
        {
            return new OptionSetValue(Convert.ToInt32(enumValue));
        }

        #endregion

        #region Guid Extensions

        /// <summary>
        /// Transforms a Guid into an EntityReference.
        /// </summary>
        /// <param name="target">The target Guid to transform into an EntityReference.</param>
        /// <param name="logicalName">The logical name of the entity for which the Guid is a reference.</param>
        /// <returns>
        /// An EntityReference with the specified Guid as the Id and logical entity name as the referenced entity, or a null
        /// value if the Guid is an empty Guid.
        /// </returns>
        public static EntityReference ToEntityReference(this Guid target, string logicalName)
        {
            return !target.Equals(Guid.Empty) ? new EntityReference(logicalName, target) : null;
        }

        /// <summary>
        /// Transforms a Guid into an EntityReference.
        /// </summary>
        /// <param name="target">The target Guid to transform into an EntityReference.</param>
        /// <param name="logicalName">The logical name of the entity for which the Guid is a reference.</param>
        /// <returns>
        /// An EntityReference with the specified Guid as the Id and logical entity name as the referenced entity, or a null
        /// value if the Guid is null.
        /// </returns>
        public static EntityReference ToEntityReference(this Guid? target, string logicalName)
        {
            return target.GetValueOrDefault().ToEntityReference(logicalName);
        }

        #endregion

        #region AliasedValue Extensions

        public static object GetValue(this AliasedValue target)
        {
            return (target != null) ? target.Value : null;
        }

        public static T GetValueOrDefault<T>(this AliasedValue target)
        {
            return target.GetValueOrDefault(default(T));
        }

        public static T GetValueOrDefault<T>(this AliasedValue target, T defaultValue)
        {
            var val = target.GetValue();
            return (val is T) ? (T)val : defaultValue;
        }

        #endregion
        
        #region Int Extensions

        /// <summary>
        /// Transforms the int value into a Money object.
        /// </summary>
        /// <param name="target">The target int value to be transformed into an OptionSetValue object.</param>
        /// <returns>An OptionSetValue object whose value will be set to the value of the target int.</returns>
        public static OptionSetValue ToOptionSetValue(this int target)
        {
            return new OptionSetValue(target);
        }

        /// <summary>
        /// Transforms the int value into an OptionSetValue object.
        /// </summary>
        /// <param name="target">The target int value to be transformed into an OptionSetValue object.</param>
        /// <returns>
        /// A OptionSetValue object whose value will be set to the value of the target int, or null if the int value is null.
        /// </returns>
        public static OptionSetValue ToOptionSetValue(this int? target)
        {
            return target.HasValue ? new OptionSetValue(target.Value) : null;
        }

        #endregion

        #region Money Extensions

        public static decimal? GetValue(this Money target)
        {
            return (target != null) ? (decimal?)target.Value : null;
        }

        public static decimal GetValueOrDefault(this Money target)
        {
            return target.GetValueOrDefault(default(decimal));
        }

        public static decimal GetValueOrDefault(this Money target, decimal defaultValue)
        {
            return target.GetValue() ?? defaultValue;
        }

        #endregion

        #region OptionSetValue Extensions

        public static int? GetValue(this OptionSetValue target)
        {
            return (target != null) ? (int?)target.Value : null;
        }

        public static int GetValueOrDefault(this OptionSetValue target)
        {
            return target.GetValueOrDefault(default(int));
        }

        public static int GetValueOrDefault(this OptionSetValue target, int defaultValue)
        {
            return target.GetValue() ?? defaultValue;
        }

        #endregion

        #region ParameterCollection Extensions

        /// <summary>
        /// Gets an item as the specified type from the <b>ParameterCollection</b>.
        /// </summary>
        /// <remarks>
        /// This method uses the <b>as</b> operator to attempt to cast the returned item to the specified type and therefore
        /// <b>T</b> is constrained to be a class.
        /// </remarks>
        /// <typeparam name="T">The type as which the item will be returned.</typeparam>
        /// <param name="pc">The <b>ParameterCollection</b> from which the item will be retrieved.</param>
        /// <param name="key">The key to the item in the <b>ParameterCollection</b>.</param>
        /// <returns>The value retrieved from the <b>ParameterCollection</b> as the specified type or null if the item cannot be found or cast to the specified type.</returns>
        public static T GetItemAs<T>(this ParameterCollection pc, string key) where T : class
        {
            if (pc.Contains(key))
            {
                return pc[key] as T;
            }

            return null;
        }

        /// <summary>
        /// Gets an item cast to the specified type from the <b>ParameterCollection</b>.
        /// </summary>
        /// <typeparam name="T">The type as which the item will be returned.</typeparam>
        /// <param name="pb">The <b>ParameterCollection</b> from which the item will be retrieved.</param>
        /// <param name="key">The key to the item in the <b>ParameterCollection</b>.</param>
        /// <param name="defaultValue">The default value to be returned if the item cannot be cast, is <b>null</b>, or cannot be found.</param>
        /// <returns>The value retrieved from the <b>ParameterCollection</b> cast as the specified type or the <b>defaultValue</b> if the item cannot be found or is null.</returns>
        public static T GetItemCastAs<T>(this ParameterCollection pc, string key, T defaultValue)
        {
            if (pc.Contains(key))
            {
                object item = pc[key];
                if (item is T)
                {
                    return (T)pc[key];
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// Gets an item converted to the specified type from the <b>ParameterCollection</b>.
        /// </summary>
        /// <remarks>
        /// The type specified for the conversion must implement the IConvertible interface, which will be the case for the built in value types.
        /// </remarks>
        /// <typeparam name="T">The type as which the item will be returned.</typeparam>
        /// <param name="pb">The <b>ParameterCollection</b> from which the item will be retrieved.</param>
        /// <param name="key">The key to the item in the <b>ParameterCollection</b>.</param>
        /// <param name="defaultValue">The default value to be returned if the item cannot be converted, is <b>null</b>, or cannot be found.</param>
        /// <returns>The value retrieved from the <b>ParameterCollection</b> converted as the specified type or the <b>defaultValue</b> if the item cannot be found or is null.</returns>
        public static T GetItemConvertedAs<T>(this ParameterCollection pc, string key, T defaultValue) where T : IConvertible
        {
            if (pc.Contains(key) && (pc[key] != null))
            {
                try
                {
                    return (T)Convert.ChangeType(pc[key], typeof(T));
                }
                catch
                {
                }
            }

            return defaultValue;
        }

        #endregion

        #region String Extensions

        /// <summary>
        /// Transforms the string into a FetchExpression.
        /// </summary>
        /// <remarks>
        /// This method does not validate the FetchXML contained in the string.  It merely creates a FetchExpression and
        /// passes the string to the FetchExpression as the Query property.
        /// </remarks>
        /// <param name="target">A string containing valid FetchXML.</param>
        /// <returns>A FetchExpression created from the target string.</returns>
        public static FetchExpression ToFetchExpression(this string target)
        {
            return new FetchExpression(target);
        }

        /// <summary>
        /// Creates a FetchExpression from a format string and a list of arguments.
        /// </summary>
        /// <remarks>
        /// The string should be a valid format string that can be passed to the string.Format method.  The supplied args are
        /// also passed to the string.Format method if they exist, otherwise the target string is used by itself.
        /// The method also makes no attempt to validate the resulting FetchXML, it merely uses the string.Format method
        /// to replace the format string placeholders and creates a new FetchExpression and sets the Query property to the
        /// resulting string.
        /// </remarks>
        /// <param name="target">
        /// A FetchXML string containing formatting placeholders to be replaced with the supplied arguments.
        /// </param>
        /// <param name="args">A list of arguments to replace the format placeholders with.</param>
        /// <returns>A FetchExpression created from the target string and list of arguments.</returns>
        public static FetchExpression ToFetchExpression(this string target, params object[] args)
        {
            FetchExpression query = null;

            if ((target != null) && (args != null))
            {
                query = new FetchExpression(string.Format(target, args));
            }
            else if (target != null)
            {
                query = new FetchExpression(target);
            }

            return query ?? new FetchExpression(null);
        }

        #endregion
    }
}
