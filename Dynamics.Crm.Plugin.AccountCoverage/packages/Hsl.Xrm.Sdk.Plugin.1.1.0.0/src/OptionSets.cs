using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hsl.Xrm.Sdk.Plugin
{
    public enum PluginAssemblyIsolationMode
    {
        None = 1,
        Sandbox = 2,
    }

    public enum SdkMessageProcessingStepMode
    {
        Synchronous = 0,
        Asynchronous = 1
    }

	public enum  SdkMessageProcessingStepStage
	{
		PreValidation = 10,
		PreOperation = 20,
		MainOperation = 30,
		PostOperation = 40,
	}
}
