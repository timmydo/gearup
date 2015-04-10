using Microsoft.Framework.Logging;
using System;
using Microsoft.Azure.Management.Redis;
using Microsoft.Azure;
using StackExchange.Redis;
using System.Threading.Tasks;
using GearUp.Models;
using Newtonsoft.Json;

namespace GearUp.Services
{
    public class RedisService
    {
		private SiteSettings _settings;
		private ILogger _logger;
		private ConnectionMultiplexer _conn;
		private IDatabase _db;
		
		public RedisService(SiteSettings settings, ILogger logger)
		{
			this._settings = settings;
			this._logger = logger;
			logger.WriteInformation("Starting Redis");
			this._conn = ConnectionMultiplexer.Connect(settings.RedisEndpoint);
			this._db = this._conn.GetDatabase();
		}

		public async Task<bool> SetAsync(string key, string data, TimeSpan? expiry = null)
		{
			if (this._db != null)
			{
				bool x = await this._db.StringSetAsync(key, data);
				return x;
			}

			return false;
		}

		public string Get(string key)
		{
			return this._db.StringGet(key);
		}

		public async Task<string> GetAsync(string key)
		{
			return await this._db.StringGetAsync(key);
		}

		public async Task<bool> CacheBuildAsync(Build b)
		{
			this._logger.WriteInformation("Redis caching build " + b.id);
			var json = JsonConvert.SerializeObject(b);
			return await this.SetAsync(b.id, json);
		}

	}
}