using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Hsl.Xrm.Sdk;
using Hsl.Xrm.Sdk.Plugin;
using Hsl.Xrm.Sdk.Plugin.Service;

namespace Fdr.Crm.Plugin.ContextLinks
{
    public class ExampleMultiServicePlugin : MultiServicePluginBase
    {
        #region Constructors/Destructors

        public ExampleMultiServicePlugin() : base() { }
        public ExampleMultiServicePlugin(string unsecureConfig, string secureConfig) : base(unsecureConfig, secureConfig) { }

        #endregion


        protected override IEnumerable<Type> GetRegisteredTypes()
        {
            return new Type[] {
				// TODO: Add multi-service plugin action types, e.g.
				// typeof(ExampleMultiServicePluginAction)
			};
        }
    }
}
