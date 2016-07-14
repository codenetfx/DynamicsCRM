using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Hsl.Xrm.Sdk;
using Hsl.Xrm.Sdk.Plugin;
using Hsl.Xrm.Sdk.Plugin.Service;


namespace Fdr.Crm.Plugin.ContextLinks.ExampleMultiServicePluginActions
{
    public class ExampleMultiServicePluginAction : MultiServicePluginActionBase<ExampleMultiServicePluginAction.Request, ExampleMultiServicePluginAction.Response>
    {
        #region Constructors

        public ExampleMultiServicePluginAction(IServicePluginContext context) : base(context) { }

        #endregion

        public override Response ExecuteAction(Request request)
        {
            // TODO: Place additional code here to implement service logic.
            var response = new Response();

            response.ResultMessage = "Success";
            response.ResultStatus = 0;

            return response;
        }

        #region Request/Response Classes

        public class Request
        {
            public string StringValue;
            public int IntValue;
            public Guid IdValue;
        }

        public class Response
        {
            public string ResultMessage;
            public int ResultStatus;
        }

        #endregion
    }
}
