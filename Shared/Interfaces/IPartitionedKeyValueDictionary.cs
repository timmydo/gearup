using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Interfaces
{
    public interface IPartitionedKeyValueDictionary : IService
    {
		// return string.empty if it doesn't exist
		Task<string> GetKeyAsync(string key);
		Task DeleteKeyAsync(string key);
		Task AddKeyAsync(string key, string value);
		Task<bool> UpdateKeyAsync(string key, string value);
    }
}
