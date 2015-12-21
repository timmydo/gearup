using Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GearUp.Test.Mocks
{
	public class MockDictionary : IPartitionedKeyValueDictionary
	{
		private Dictionary<string, string> _dict = new Dictionary<string,string>();

		public async Task AddKeyAsync(string key, string value)
		{
			await Task.FromResult(0);
			if (_dict.ContainsKey(key))
			{
				throw new Exception("key exists in dict: " + key);
			}

			_dict.Add(key, value);
		}

		public async Task DeleteKeyAsync(string key)
		{
			await Task.FromResult(0);
			if (!_dict.ContainsKey(key))
			{
				throw new Exception("key not found in dict: " + key);
			}

			_dict.Remove(key);
		}

		public async Task<string> GetKeyAsync(string key)
		{
			await Task.FromResult(0);
			if (!_dict.ContainsKey(key)) return null;
			return _dict[key];
		}

		public async Task<bool> UpdateKeyAsync(string key, string value)
		{
			await Task.FromResult(0);
			_dict[key] = value;
			return true;
		}
	}
}
