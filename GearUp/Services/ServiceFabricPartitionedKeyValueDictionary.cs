
namespace GearUp.Services
{
	using Microsoft.ServiceFabric.Services.Remoting.Client;
	using Shared.Helpers;
	using Shared.Interfaces;
	using System;
	using System.Threading.Tasks;

	public class ServiceFabricPartitionedKeyValueDictionary : IPartitionedKeyValueDictionary
	{
		private Uri uri;

		public ServiceFabricPartitionedKeyValueDictionary(Uri uri)
		{
			this.uri = uri;
		}

		public async Task AddKeyAsync(string key, string value)
		{
			var proxy = ServiceProxy.Create<IPartitionedKeyValueDictionary>(FNV.Hash64(key), this.uri);
			await proxy.AddKeyAsync(key, value);
		}

		public async Task DeleteKeyAsync(string key)
		{
			var proxy = ServiceProxy.Create<IPartitionedKeyValueDictionary>(FNV.Hash64(key), this.uri);
			await proxy.DeleteKeyAsync(key);
		}

		public async Task<IKeyValueEntity> GetKeyAsync(string key)
		{
			var proxy = ServiceProxy.Create<IPartitionedKeyValueDictionary>(FNV.Hash64(key), this.uri);
			return await proxy.GetKeyAsync(key);
		}
	}
}