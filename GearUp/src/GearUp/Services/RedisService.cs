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

		public async Task ForgetAsync(string key)
		{
			await this._db.KeyDeleteAsync(key);
		}

		public async Task<string> GetAsync(string key)
		{
			return await this._db.StringGetAsync(key);
		}

		public bool CacheBuild(Build b)
		{
			this._logger.WriteInformation("Redis caching build " + b.id);
			var json = JsonConvert.SerializeObject(b);
			return this._db.StringSet(b.id, json);
		}
		public bool CacheList(BuildList list)
		{
			this._logger.WriteInformation("Redis caching list " + list.id);
			var json = JsonConvert.SerializeObject(list);
			return this._db.StringSet(list.id, json);
		}

		public Build GetBuild(string id)
		{
			var json = this.Get(id);
			if (json == null) return null;

			Build b = JsonConvert.DeserializeObject<Build>(json);
			if (b.DocType == "build")
			{
				this._logger.WriteInformation("Redis Found Build: " + id);
				return b;
			}

			return null;
		}

		public BuildList GetList(string id)
		{
			var json = this.Get(id);
			if (json == null) return null;

			BuildList list = JsonConvert.DeserializeObject<BuildList>(json);
			if (list.DocType == "list")
			{
				this._logger.WriteInformation("Redis Found List: " + id);
				return list;
			}

			return null;
		}


		public async Task<Build[]> GetUserBuildsAsync(string id)
		{
			var json = await this.GetAsync("userbuilds/" + id);
			if (json == null) return null;

			var list = JsonConvert.DeserializeObject<Build[]>(json);
			if (list != null)
			{
				this._logger.WriteInformation("Redis Found User builds for " + id);
			}
			return list;
		}

		public async Task<BuildList[]> GetUserListsAsync(string id)
		{
			var json = await this.GetAsync("userlists/" + id);
			if (json == null) return null;

			var list = JsonConvert.DeserializeObject<BuildList[]>(json);
			if (list != null)
			{
				this._logger.WriteInformation("Redis Found User lists for " + id);
			}
			return list;
		}

		public async Task<bool> CacheUserBuildsAsync(string uid, Build[] list)
		{
			this._logger.WriteInformation("Redis caching user builds for " + uid);
			var json = JsonConvert.SerializeObject(list);
			return await this._db.StringSetAsync("userbuilds/" + uid, json);
		}
		public async Task<bool> CacheUserListsAsync(string uid, BuildList[] list)
		{
			this._logger.WriteInformation("Redis caching user lists for " + uid);
			var json = JsonConvert.SerializeObject(list);
			return await this._db.StringSetAsync("userlists/" + uid, json);
		}


	}
}