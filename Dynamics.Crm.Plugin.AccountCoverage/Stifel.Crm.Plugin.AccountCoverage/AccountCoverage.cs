using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Hsl.Xrm.Sdk;
using Hsl.Xrm.Sdk.Plugin;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System.Text.RegularExpressions;

namespace Stifel.Crm.Plugin.ShortnameAccount
{
    /// <summary>
    /// This plugin will update account coverage records for the associated Company based on Shortname lookup field
    /// </summary>
    /// <remarks>
    /// Intended registration.
    ///     Entity: AccountCoverage
    ///     Message: Create
    ///     Stage: Pre-Operation
    ///     Mode: Synchronous
    ///         
    /// Intended registration.
    ///     Entity: AccountCoverage
    ///     Message: update
    ///     Stage: Pre-Operation
    ///     Mode: Synchronous
    ///     Image: Pre-Image (Target)
    ///     
    /// </remarks>    
    public class AccountCoverage : PluginBase, IPlugin
    {
        
        #region Constructors/Destructors

        public AccountCoverage() : base() { }
        public AccountCoverage(string unsecureConfig, string secureConfig) : base(unsecureConfig, secureConfig) { }

        #endregion

        public override void ExecuteAction()
        {
            // Verify that the target entity represents the Account Coverage entity.
            // If not, this plug-in was not registered correctly.

            string tradeEntityName = "sfc_trade";
            string accountCoverageEntityName = "sfc_accountcoverage";

            if (this.Context.PluginContext.PrimaryEntityName != accountCoverageEntityName) { throw new InvalidOperationException("The Stifel.Crm.Plugins.ShortnameAccount plugin is only valid for registration on the [" + accountCoverageEntityName + "] entity."); }

            // Get an entity with the latest values for each attribute
            var targetEntity = this.Context.TargetPreImage.Merge(this.Context.TargetInput);

            string clientStatusAttributeName = "sfc_clientstatuscode";
            string repFieldSchemaName = "ownerid";
            string commissionFieldSchemaName = "sfc_commissionrep";
            string shortNameFieldSchemaName = "sfc_accountcoverage";
            string setUnderReviewFlagAttributeName = "sfc_sa_setunderreview";


            string repFieldValue = targetEntity.GetFieldValueOrDefault(repFieldSchemaName, String.Empty);
            string clientStatusFieldValue = targetEntity.GetFieldValueOrDefault(clientStatusAttributeName, String.Empty);
            string shortNameFieldValue = targetEntity.GetFieldValueOrDefault(shortNameFieldSchemaName, String.Empty);
            

            var targetInput = this.Context.TargetInput;

            decimal commissionFieldValue = 0;


            if (repFieldValue != "N/A")
            {
                //Get shortname/rep records from the sfc_trade table              
                EntityCollection tradeEntityRecords = this.Context.UserOrgService.RetrieveMultiple(
                tradeEntityName,
                new ColumnSet(true),
                new ConditionExpression[] {
                    new ConditionExpression(shortNameFieldSchemaName, ConditionOperator.Equal, shortNameFieldValue),
                    new ConditionExpression(repFieldSchemaName, ConditionOperator.Equal, repFieldValue)
                },
                LogicalOperator.And
                );

               
                if ((tradeEntityRecords != null) && (tradeEntityRecords.Entities.Count > 0))
                {
                    foreach (Entity tradeEntity in tradeEntityRecords.Entities)
                    {

                        //Read commission field value
                        commissionFieldValue = Convert.ToDecimal(tradeEntity.Attributes[commissionFieldSchemaName].ToString());

                        // Active:
                        // If ‘Rep’ field does not = N/A team value, and there is a commission value > 0 for the corresponding rep/shortname 
                        // combo in the trade table, set the value to Active
                        if ((Convert.ToDecimal(commissionFieldValue) > 0) && (repFieldValue != "N/A"))
                        {
                            tradeEntity.Attributes[clientStatusAttributeName] = "Active";
                            this.Context.UserOrgService.Update(tradeEntity);
                        }

                        // Prospect:
                        // If ‘Rep’ field does not = N/A team value, and there is a commission value <= 0 for the corresponding rep/shortname 
                        // combo in the trade table, set the value to Prospect
                        if ((Convert.ToDecimal(commissionFieldValue) <= 0) && (repFieldValue != "N/A"))
                        {
                            tradeEntity.Attributes[clientStatusAttributeName] = "Prospect";
                            this.Context.UserOrgService.Update(tradeEntity);
                        }
                    }

                }
            }
            
            //  Rotated:
            //  If ‘Rep’ field = N/A team value, set status to Rotated
            if (repFieldValue == "N/A")
            {
                targetInput[clientStatusAttributeName] = "Rotated";
            }

            // Under Review:
            // If client status is manually set to ‘Under Review’ then set the status to ‘Under Review’ 
            // for any additional Account Coverage records with the same ‘shortname/rep’ combo

            if (clientStatusFieldValue == "Under Review")
            {

                // Check if sfc_sa_setunderreview flag is set, update client status accordingly
                string setUnderReviewFlag = targetEntity.GetFieldValueOrDefault(setUnderReviewFlagAttributeName, Boolean.FalseString);

                if ((setUnderReviewFlag == "true"))
                {
                    targetInput[clientStatusAttributeName] = "Under Review";
                    return;
                }

                EntityCollection accountCoverageEntityRecords = this.Context.UserOrgService.RetrieveMultiple(
                accountCoverageEntityName,
                new ColumnSet(true),
                new ConditionExpression[] {
                    new ConditionExpression(shortNameFieldSchemaName, ConditionOperator.Equal, shortNameFieldValue),
                    new ConditionExpression(repFieldSchemaName, ConditionOperator.Equal, repFieldValue)
                },
                LogicalOperator.And
                );
                
                foreach (Entity accountCoverageEntity in accountCoverageEntityRecords.Entities)
                {

                    // Under Review:
                    // If client status is manually set to ‘Under Review’ then set the status to ‘Under Review’ 
                    // for any additional Account Coverage records with the same ‘shortname/rep’ combo
                    accountCoverageEntity.Attributes[clientStatusAttributeName] = "Under Review";
                    accountCoverageEntity.Attributes[setUnderReviewFlagAttributeName] = "Under Review";
                    this.Context.UserOrgService.Update(accountCoverageEntity);
                }
            }
      
                

        }
    }
}
