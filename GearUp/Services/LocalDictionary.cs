using Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GearUp.Services
{
	public class LocalDictionary : IPartitionedKeyValueDictionary
	{
		private Dictionary<string, string> _dict = new Dictionary<string,string>();

		private class LocalDictionaryKey : IKeyValueEntity
		{
			public string Key { get; set; }
			public string Value { get; set; }
			private LocalDictionary _dict { get; set; }

			public LocalDictionaryKey(LocalDictionary d)
			{
				_dict = d;
			}

			public async Task<bool> UpdateAsync()
			{
				return await _dict.UpdateKeyAsync(this.Key, this.Value);
			}
		}


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

		public async Task<IKeyValueEntity> GetKeyAsync(string key)
		{
			await Task.FromResult(0);
			if (!_dict.ContainsKey(key)) return null;
			return new LocalDictionaryKey(this)
			{
				Key = key,
				Value = _dict[key]
			};
		}

		private async Task<bool> UpdateKeyAsync(string key, string value)
		{
			await Task.FromResult(0);
			_dict[key] = value;
			return true;
		}
	}
}
