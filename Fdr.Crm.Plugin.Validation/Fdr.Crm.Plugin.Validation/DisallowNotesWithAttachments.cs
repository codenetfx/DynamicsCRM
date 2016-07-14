using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hsl.Xrm.Sdk;
using Hsl.Xrm.Sdk.Plugin;
using Microsoft.Xrm.Sdk;

namespace Fdr.Crm.Plugin.Validation
{
    /// <summary>
    /// This plugin will restrict the creation of notes/annotations if there are any attachments.
    /// the conditions related to the requirement.
    /// </summary>
    /// <remarks>
    /// Intended registration.
    ///     Entity: annotation
    ///     Message: Create
    ///     Stage: Pre-Validation
    ///     Mode: Synchronous
    ///         
    /// </remarks>    
    public class DisallowNotesWithAttachments : PluginBase, IPlugin
    {
        #region Constructors/Destructors

        public DisallowNotesWithAttachments() : base() { }
        public DisallowNotesWithAttachments(string unsecureConfig, string secureConfig) : base(unsecureConfig, secureConfig) { }

        #endregion

        public override void ExecuteAction()
        {
            // Verify that the target entity represents an account.
            // If not, this plug-in was not registered correctly.
            if (this.Context.PluginContext.PrimaryEntityName != "annotation") { return; }

            // If we don't have a document body then we can exit
            var docBody = this.Context.TargetInput.GetFieldValueOrDefault<string>("documentbody");
            if (string.IsNullOrEmpty(docBody)) { return; }

            // If we made it this far then there is an attachment and we need to throw an error.
            this.Context.Throw("Attachment upload has been disabled for annotations.", null);

        }
    }
}
