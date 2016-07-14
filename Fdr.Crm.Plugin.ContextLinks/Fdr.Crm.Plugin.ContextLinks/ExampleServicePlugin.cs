using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Hsl.Xrm.Sdk;
using Hsl.Xrm.Sdk.Plugin;
using Hsl.Xrm.Sdk.Plugin.Service;


namespace Fdr.Crm.Plugin.ContextLinks
{
    public class ExampleServicePlugin : ServicePluginBase<ExampleServicePlugin.Request, ExampleServicePlugin.Response>
    {
        #region Constructors/Destructors

        public ExampleServicePlugin() : base() { }
        public ExampleServicePlugin(string unsecureConfig, string secureConfig) : base(unsecureConfig, secureConfig) { }

        #endregion

        public override ExampleServicePlugin.Response ExecuteService(Request request)
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
