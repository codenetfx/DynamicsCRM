using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hsl.Xrm.Sdk;
using Hsl.Xrm.Sdk.Plugin;
using Microsoft.Xrm.Sdk;

namespace Hsl.Crm.Plugin.Project1
{
    public class AnnotationAttachmentValidationePlugin : PluginBase, IPlugin
    {
        #region Constructors/Destructors

        public AnnotationAttachmentValidationePlugin() : base() { }
        public AnnotationAttachmentValidationePlugin(string unsecureConfig, string secureConfig) : base(unsecureConfig, secureConfig) { }

        #endregion

        public override void ExecuteAction()
        {
            //TODO: Register plugin Pre-Update

            //Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            // The InputParameters collection contains all the data passed in the message request.
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.
                Entity entity = (Entity)context.InputParameters["Target"];

                // Verify that the target entity represents an account.
                // If not, this plug-in was not registered correctly.
                if (entity.LogicalName != "annotation")
                    return;

                try
                {
                    //if attachment was added, clear it out
                    if (entity.Attributes["Document Body"] != null)
                    {
                        entity.Attributes["Document Body"] = string.Empty;
                        entity.Attributes["File Name"] = string.Empty;
                        entity.Attributes["File Size"] = 0;

                       throw new InvalidPluginExecutionException("Attachment upload has been disabled for annotations.", ex);
                    }

                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred during Document Body validation.", ex);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("ValidationPlugin: {0}", ex.ToString());
                    throw;
                }


            }
        }
    }
}
