using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Interfaces
{
    public interface IPartitionedKeyValueDictionary
    {
		// return string.empty if it doesn't exist
		Task<IKeyValueEntity> GetKeyAsync(string key);
		Task DeleteKeyAsync(string key);
		Task AddKeyAsync(string key, string value);
    }
}
