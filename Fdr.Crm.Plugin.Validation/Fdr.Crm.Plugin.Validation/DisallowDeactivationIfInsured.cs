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
    public class DisallowDeactivationIfInsured : PluginBase, IPlugin
    {
        #region Constructors/Destructors

        public DisallowDeactivationIfInsured() : base() { }
        public DisallowDeactivationIfInsured(string unsecureConfig, string secureConfig) : base(unsecureConfig, secureConfig) { }

        #endregion

        public override void ExecuteAction()
        {
            // TODO:  This is currently on hold as Federated is deciding what "Insured" means for a contact/company.  Once
            //        that decision is made this plugin can be implemented.


            // Company
            // fdr_pcstatusid
            // fdr_wcstatusid
            // fdr_groupstatusid
            // fdr_lifestatusid
            // fdr_distatusid
            // fdr_iastatusid

            // Contact
            // fdr_lifestatusid
            // fdr_distatusid
            // fdr_iastatusid


            // Determine the attribute to use for filtering
            //string affPartnerParentField = null;
            //if (Context.PluginContext.PrimaryEntityName.Equals("account"))
            //{
            //    affPartnerParentField = "fdr_companyid";
            //}
            //else if (Context.PluginContext.PrimaryEntityName.Equals("contact"))
            //{
            //    affPartnerParentField = "fdr_contactid";
            //}
            //else
            //{
            //    Context.Throw("Invalid plugin registration, this plugin must be registered for account and contact only.", null);
            //}

            //// Determine if the entity is being deactivated
            //var newState = Context.TargetInput.GetPicklistValueOrDefault("statecode", -1);
            //var oldState = Context.TargetPreImage.GetPicklistValueOrDefault("statecode", -1);
            //if (!oldState.Equals(0) || !newState.Equals(1)) {
            //    return; // Not being deactivated so don't care
            //}

            //// Determine number of related affinity partners
            //var affPartnerCount = Context.SystemOrgService.RetrieveMultiple(
            //    "fdr_affinitypartnerrelationship",
            //    new ColumnSet("fdr_affinitypartnerrelationshipid"),
            //    affPartnerParentField,
            //    ConditionOperator.Equal, Context.PluginContext.PrimaryEntityId).Entities.Count;

            //if (affPartnerCount > 0)
            //{
            //    Context.Throw("Invalid deactivation, deactivation of records with related affinity partners is not allowed.", null);
            //}
        }

    }
}
