
namespace GearUp.Services
{
	using Microsoft.ServiceFabric.Services.Remoting.Client;
	using Shared.Interfaces;
	using System;
	using System.Threading.Tasks;

	public class ServiceFabricPartitionedKeyValueDictionary : IPartitionedKeyValueDictionary
	{
		private Uri uri;


		private const ulong fnvPrime = unchecked(1099511628211);
		private const ulong fnvOffsetBasis = unchecked(14695981039346656037);

		static private long getPartitionKey(string key)
		{
			ulong h = fnvOffsetBasis;
			if (string.IsNullOrEmpty(key)) return unchecked((long)h);
			var arr = key.ToCharArray();
			for (var i = 0; i < arr.Length; i++)
			{
				unchecked
				{
					h ^= arr[i];
					h *= fnvPrime;
				}
			}

			return unchecked((long)h);
		}


		public ServiceFabricPartitionedKeyValueDictionary(Uri uri)
		{
			this.uri = uri;
		}


		public async Task AddKeyAsync(string key, string value)
		{
			var proxy = ServiceProxy.Create<IPartitionedKeyValueDictionary>(getPartitionKey(key), this.uri);
			await proxy.AddKeyAsync(key, value);
		}

		public async Task DeleteKeyAsync(string key)
		{
			var proxy = ServiceProxy.Create<IPartitionedKeyValueDictionary>(getPartitionKey(key), this.uri);
			await proxy.DeleteKeyAsync(key);
		}

		public async Task<string> GetKeyAsync(string key)
		{
			var proxy = ServiceProxy.Create<IPartitionedKeyValueDictionary>(getPartitionKey(key), this.uri);
			return await proxy.GetKeyAsync(key);
		}

		public async Task UpdateKeyAsync(string key, string value, string updateFunc)
		{
			var proxy = ServiceProxy.Create<IPartitionedKeyValueDictionary>(getPartitionKey(key), this.uri);
			await proxy.UpdateKeyAsync(key, value, updateFunc);
		}
	}
}