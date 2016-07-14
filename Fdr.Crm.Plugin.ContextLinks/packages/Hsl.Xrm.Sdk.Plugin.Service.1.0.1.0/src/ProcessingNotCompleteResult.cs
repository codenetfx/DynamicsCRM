using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hsl.Xrm.Sdk.Plugin.Service
{
	internal class ProcessingNotCompleteResult
	{
		public ProcessingNotCompleteResult()
		{
		}
		public ProcessingNotCompleteResult(object data)
		{
			Data = data;
		}
		public object Data { get; set; }
	}
}
