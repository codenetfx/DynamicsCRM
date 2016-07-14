using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hsl.Xrm.Sdk;
using Hsl.Xrm.Sdk.Plugin;
using Microsoft.Xrm.Sdk;

namespace Fdr.Crm.Plugin.MCSS
{
    /// <summary>
    /// This plugin will update MCSS task type, due date and planning date.
    /// </summary>
    /// <remarks>
    /// Intended registration.
    ///     Entity: opportunity
    ///     Message: Create
    ///     Stage: Pre-Validation
    ///     Mode: Synchronous
    ///         
    /// Intended registration.
    ///     Entity: opportunity
    ///     Message: update
    ///     Stage: Pre-Validation
    ///     Mode: Synchronous
    ///     Image: Pre-Image (Target)
    ///     Attributes: All RCR, FPR, ACR, CCP date fields for both pre-image and entity.
    ///     
    /// </remarks>    
    public class SetMcssNextTask : PluginBase, IPlugin
    {
        private static readonly List<string> McssTaskTypes = new List<string>() { "rcr", "fpr", "acr", "ccp" };

        #region Constructors/Destructors

        public SetMcssNextTask() : base() { }
        public SetMcssNextTask(string unsecureConfig, string secureConfig) : base(unsecureConfig, secureConfig) { }

        #endregion

        public override void ExecuteAction()
        {
            // Verify that the target entity represents an opportunity.
            // If not, this plug-in was not registered correctly.
            // TODO: throw an InvalidPluginExecutionException with message indicating invalid registration.
            if (this.Context.PluginContext.PrimaryEntityName != "opportunity") { throw new InvalidOperationException("The Fdr.Crm.Plugin.MCSS.SetMcssNextTask plugin is only valid for registrationon the [opportunity] entity."); }

            string nextTask = string.Empty;
            string separator = string.Empty;

            // Get an entity with the latest values for each attribute
            var targetEntity = this.Context.TargetPreImage.Merge(this.Context.TargetInput);

            DateTime tomorrow = DateTime.Today.AddDays(1);

            // Create a list for building the MCSS next task
            var mcssNextDates = new List<KeyValuePair<DateTime, string>>();
            var mcssNextPlanningDates = new List<KeyValuePair<DateTime, string>>();
            var mcssNextScheduledDates = new List<KeyValuePair<DateTime, string>>();
            var mcssNextDueDates = new List<KeyValuePair<DateTime, string>>();

            // Add each of the MCSS types to sorted lists to get the next MCSS date for each type
            foreach (var mcssName in McssTaskTypes) {

                // Skip this item if there is a completed date.
                DateTime completedDate = targetEntity.GetFieldValueOrDefault("fdr_{0}completedate".Substitute(mcssName), DateTime.MinValue);
                if (completedDate != DateTime.MinValue) { continue; }

                // Add planning dates to the sorted lists if in the past or today
                var planningDate = targetEntity.GetFieldValueOrDefault("fdr_{0}planningdate".Substitute(mcssName), DateTime.MinValue);
                if ((planningDate != DateTime.MinValue) && (planningDate < tomorrow))
                {
                    mcssNextDates.Add(new KeyValuePair<DateTime,string>(planningDate, mcssName));
                    mcssNextPlanningDates.Add(new KeyValuePair<DateTime,string>(planningDate, mcssName));
                }

                // Add scheduled dates to the sorted lists if in the past or today
                var scheduledDate = targetEntity.GetFieldValueOrDefault("fdr_{0}scheduleddate".Substitute(mcssName), DateTime.MinValue);
                if ((scheduledDate != DateTime.MinValue) && (scheduledDate < tomorrow))
                {
                    mcssNextDates.Add(new KeyValuePair<DateTime,string>(scheduledDate, mcssName));
                    mcssNextScheduledDates.Add(new KeyValuePair<DateTime,string>(scheduledDate, mcssName));
                }

                // Add due dates to the sorted lists if in the past or today
                var dueDate = targetEntity.GetFieldValueOrDefault("fdr_{0}duedate".Substitute(mcssName), DateTime.MinValue);
                if ((dueDate != DateTime.MinValue) && (dueDate < tomorrow))
                {
                    mcssNextDates.Add(new KeyValuePair<DateTime,string>(dueDate, mcssName));
                    mcssNextDueDates.Add(new KeyValuePair<DateTime,string>(dueDate, mcssName));
                }
            }

            //Build the next MCSS next task type string based on ascending order of all dates.
            var mcssNextTask = StringExtensions.JoinIfNotNullOrWhiteSpace(
                "/", 
                false, 
                null,
                mcssNextDates.OrderBy(x => x.Key).Select(x => x.Value.ToUpper()).Distinct().ToArray());
         

            // Get the next dates for each date type based on each task type, taking the most recent date for each type
            var mcssNextDueDate = mcssNextDueDates.OrderByDescending(x => x.Key).Select(x => (DateTime?)x.Key).DefaultIfEmpty(null).FirstOrDefault();
            var mcssNextPlanningDate = mcssNextPlanningDates.OrderByDescending(x => x.Key).Select(x => (DateTime?)x.Key).DefaultIfEmpty(null).FirstOrDefault();
            var mcssNextScheduledDate = mcssNextScheduledDates.OrderByDescending(x => x.Key).Select(x => (DateTime?)x.Key).DefaultIfEmpty(null).FirstOrDefault();

            // Set the values on the target input so the MCSS fields get updated
            var targetInput = this.Context.TargetInput;
            targetInput["fdr_nextcsstasktype"] = mcssNextTask;
            targetInput["fdr_mcssduedate"] = mcssNextDueDate;
            targetInput["fdr_nextcsstaskplanningdate"] = mcssNextPlanningDate;
            targetInput["fdr_nextcsstaskscheduleddate"] = mcssNextScheduledDate;
            
        
        }
    }
}
