using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Hsl.Xrm.Sdk;
using Hsl.Xrm.Sdk.Plugin;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System.Text.RegularExpressions;

namespace Stifel.Crm.Plugins
{
    /// <summary>
    /// This plugin will maintain Client Status on Account Coverage and trigger on Account entity
    /// </summary>
    /// <remarks>
    /// Intended registration.
    ///     Entity: Account 
    ///     Message: Create
    ///     Stage: Pre-Operation
    ///     Mode: Synchronous
    ///         
    /// Intended registration.
    ///     Entity: Account 
    ///     Message: update
    ///     Stage: Pre-Operation
    ///     Mode: Synchronous
    ///     Image: Pre-Image (Target)
    ///     
    /// </remarks>    
    public class AccountShortName : PluginBase, IPlugin
    {
        
        #region Constructors/Destructors

        public AccountShortName() : base() { }
        public AccountShortName(string unsecureConfig, string secureConfig) : base(unsecureConfig, secureConfig) { }

        #endregion

        public override void ExecuteAction()
        {
            // Verify that the target entity represents the Account Coverage entity.
            // If not, this plug-in was not registered correctly.

            string tradeEntityName = "sfc_trade";
            string accountCoverageEntityName = "sfc_accountcoverage";

            if (this.Context.PluginContext.PrimaryEntityName != accountCoverageEntityName) { throw new InvalidOperationException("The Stifel.Crm.Plugins.AccountCoverage plugin is only valid for registration on the [" + accountCoverageEntityName + "] entity."); }

            // Get an entity with the latest values for each attribute
            var targetEntity = this.Context.TargetPreImage.Merge(this.Context.TargetInput);

            //TODO
            string clientStatusAttributeName = "sfc_clientstatuscode";
            string setUnderReviewFlagAttributeName = "stifel_sa_setunderreview"; //TODO
            string repFieldSchemaName = "ownerid";
            string shortNameFieldSchemaName = "sfc_tradeaccountid";

      

            string clientStatusFieldValue = targetEntity.GetFieldValueOrDefault(clientStatusAttributeName, String.Empty);
            string repFieldValue = targetEntity.GetFieldValueOrDefault(repFieldSchemaName, String.Empty);
            string shortNameFieldValue = targetEntity.GetFieldValueOrDefault(shortNameFieldSchemaName, String.Empty);


            //Get status shortname/rep records from the sfc_trade table              
            var accountClientStatus = this.Context.UserOrgService.RetrieveMultiple(
            "Account",
            new ColumnSet(true),
            new ConditionExpression[] {
                    new ConditionExpression(shortNameFieldSchemaName, ConditionOperator.Equal, shortNameFieldValue)                    
                },
                LogicalOperator.And
            ).Entities
              .FirstOrDefault()
              .GetFieldValueOrDefault<string>(clientStatusAttributeName);
                    
            if (accountClientStatus == "Inactive")
            {
                // update account coverage record
                this.Context.TargetInput[clientStatusAttributeName] = "Inactive";

            }
                
            if (accountClientStatus == "Failed/Merged")
            {
                // update account coverage record
                this.Context.TargetInput[clientStatusAttributeName] = "Failed/Merged";
            } 
                

        }
    }
}
