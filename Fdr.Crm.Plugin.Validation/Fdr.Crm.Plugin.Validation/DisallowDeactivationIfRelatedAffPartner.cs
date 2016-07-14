using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hsl.Xrm.Sdk;
using Hsl.Xrm.Sdk.Plugin;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System.Text.RegularExpressions;

namespace Fdr.Crm.Plugin.Validation
{
    /// <summary>
    /// This plugin will restrict the deactivation of accounts or contacts if there are any related affinity partners that satisfy
    /// the conditions related to the requirement.
    /// </summary>
    /// <remarks>
    /// Intended registration.
    ///     Entity: account
    ///     Message: Update
    ///     Stage: Pre-Operation
    ///     Mode: Synchronous
    ///     Filtering Attributes: statecode
    ///     PreImage:
    ///         Name: "Target"
    ///         Attributes: statecode
    ///         
    ///     Entity: contact
    ///     Message: Update
    ///     Stage: Pre-Operation
    ///     Mode: Synchronous
    ///     Filtering Attributes: statecode
    ///     PreImage:
    ///         Name: "Target"
    ///         Attributes: statecode
    ///         
    /// </remarks>
    public class DisallowDeactivationIfRelatedAffPartner : PluginBase, IPlugin
    {
        #region Constructors/Destructors

        public DisallowDeactivationIfRelatedAffPartner() : base() { }
        public DisallowDeactivationIfRelatedAffPartner(string unsecureConfig, string secureConfig) : base(unsecureConfig, secureConfig) { }

        #endregion

        public override void ExecuteAction()
        {
            // Determine the conditions needed to query for relevant related affinity partners.
            ConditionExpression[] affPartnerConditions = null;
            if (Context.PluginContext.PrimaryEntityName.Equals("account"))
            {
                affPartnerConditions = new ConditionExpression[] {
                    new ConditionExpression("fdr_companyid", ConditionOperator.Equal, Context.PluginContext.PrimaryEntityId),
                    new ConditionExpression("statecode", ConditionOperator.Equal, 0)
                };
            }
            else if (Context.PluginContext.PrimaryEntityName.Equals("contact"))
            {
                affPartnerConditions = new ConditionExpression[] {
                    new ConditionExpression("fdr_contactid", ConditionOperator.Equal, Context.PluginContext.PrimaryEntityId),
                    new ConditionExpression("statecode", ConditionOperator.Equal, 0),
                    new ConditionExpression("fdr_iskdm", ConditionOperator.Equal, true)
                };
            }
            else
            {
                Context.Throw("Invalid plugin registration, this plugin must be registered for account and contact only.", null);
            }

            // Determine if the entity is being deactivated
            var newState = Context.TargetInput.GetPicklistValueOrDefault("statecode", -1);
            var oldState = Context.TargetPreImage.GetPicklistValueOrDefault("statecode", -1);
            if (!oldState.Equals(0) || !newState.Equals(1)) {
                return; // Not being deactivated so don't care
            }

            // Determine if we have any related affinity partners that would restrict the deactivation.
            var affPartnerCount = Context.SystemOrgService.RetrieveMultiple(
                "fdr_affinitypartnerrelationship",
                new ColumnSet("fdr_affinitypartnerrelationshipid"),
                affPartnerConditions,
                LogicalOperator.And).Entities.Count;
                
            // If there are any related records returned by the query then we need to restrict deactivation.
            if (affPartnerCount > 0)
            {
                Context.Throw("Invalid deactivation, deactivation of records with related affinity partners is not allowed.", null);
            }
        }

    }
}
