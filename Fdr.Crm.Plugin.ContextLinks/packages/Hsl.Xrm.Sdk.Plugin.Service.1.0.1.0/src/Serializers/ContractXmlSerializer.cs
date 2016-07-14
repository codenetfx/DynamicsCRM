using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Hsl.Xrm.Sdk.Plugin.Service.Serializers
{
	/// <summary>
	/// Wrapper of DataContractJsonSerializer that implements ISerializer
	/// </summary>
	public class ContractXmlSerializer : ISerializer
	{
		private readonly IEnumerable<Type> _knownTypes;

		public ContractXmlSerializer()
			: this(Type.EmptyTypes)
		{
		}
		public ContractXmlSerializer(IEnumerable<Type> knownTypes)
		{
			_knownTypes = knownTypes;
		}

		public T Parse<T>(string data)
		{
			// I hate DataContractJsonSerializer but it's all that we have in partial trust without
			// a third party library. 
			var serializer = new DataContractSerializer(typeof(T), _knownTypes);
			using (var ms = new MemoryStream())
			{
				var strBinary = Encoding.UTF8.GetBytes(data);
				ms.Write(strBinary, 0, strBinary.Length);
				ms.Position = 0;
				return (T)serializer.ReadObject(ms);
			}
		}

		public string Serialize<T>(T data)
		{
			var serializer = new DataContractSerializer(typeof(T), _knownTypes);
			using (var ms = new MemoryStream())
			using (var reader = new StreamReader(ms))
			{
				serializer.WriteObject(ms, data);
				ms.Position = 0;
				return reader.ReadToEnd();
			}
		}
	}

}
