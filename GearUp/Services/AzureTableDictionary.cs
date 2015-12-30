namespace GearUp.Services
{
	using GearUp.Models;
	using Microsoft.Extensions.Logging;
	using Microsoft.Extensions.OptionsModel;
	using Shared.Interfaces;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Microsoft.WindowsAzure.Storage.Table;
	using Microsoft.WindowsAzure.Storage;

	public class AzureTableDictionary : IPartitionedKeyValueDictionary
	{
		private ILogger _logger;
		private readonly CloudTable _table;

		private class InternalAzureTableEntity : TableEntity
		{
			public string Value { get; set; }
		}

		private class AzureTableEntity : IKeyValueEntity
		{
			public string Key { get; set; }
			public string Value { get; set; }

			private InternalAzureTableEntity _obj;
			private CloudTable _table;

			public AzureTableEntity(InternalAzureTableEntity o, CloudTable t)
			{
				this._obj = o;
				this._table = t;
			}

			public async Task<bool> UpdateAsync()
			{
				_obj.Value = Value;
				var op = TableOperation.Replace(_obj);
				var res = await _table.ExecuteAsync(op);
				return res.HttpStatusCode >= 200 && res.HttpStatusCode < 300;
			}
		}

		private static string EscapeKey(string key)
		{
			return key.Replace('/', '$');
		}

		public AzureTableDictionary(IOptions<SiteSettings> settings, ILogger logger)
		{
			this._logger = logger;
			logger.LogInformation("Azure Table creation start");

			var acct = CloudStorageAccount.Parse(settings.Value.StorageConnectionString);
			var client = acct.CreateCloudTableClient();
			this._table = client.GetTableReference(settings.Value.TableStorageTableName);
			this._table.CreateIfNotExists();

			logger.LogInformation("Azure Table started");
		}

		public async Task AddKeyAsync(string key, string value)
		{
			var obj = new InternalAzureTableEntity()
			{
				PartitionKey = EscapeKey(key),
				RowKey = string.Empty,
				Value = value
			};
			var op = TableOperation.Insert(obj);
			await _table.ExecuteAsync(op);
		}

		public async Task DeleteKeyAsync(string key)
		{
			var obj = new InternalAzureTableEntity()
			{
				PartitionKey = EscapeKey(key),
				RowKey = string.Empty
			};
			var op = TableOperation.Delete(obj);
			await _table.ExecuteAsync(op);
		}

		public async Task<IKeyValueEntity> GetKeyAsync(string key)
		{
			var op = TableOperation.Retrieve<InternalAzureTableEntity>(EscapeKey(key), string.Empty);
			var res = await _table.ExecuteAsync(op);
			var returnedObj = (InternalAzureTableEntity)res.Result;
			if (returnedObj != null)
			{
				return new AzureTableEntity(returnedObj, _table)
				{
					Key = key,
					Value = returnedObj.Value
				};
			}
			else
			{
				return null;
			}
		}

	}
}
