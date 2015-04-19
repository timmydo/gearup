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
	public class RedisWrapper
	{
		private readonly ConnectionMultiplexer _conn;
		private readonly IDatabase _db;
		private readonly string _keyprefix;


		public RedisWrapper(SiteSettings settings)
		{
			this._conn = ConnectionMultiplexer.Connect(settings.RedisEndpoint);
			this._db = this._conn.GetDatabase();
			this._keyprefix = settings.DocumentDatabaseId + "/" + settings.DocumentCollectionId + "/";

		}

		public async Task ForgetAsync(string key)
		{
			await this._db.KeyDeleteAsync(this._keyprefix + key);
		}

		public async Task<string> GetAsync(string key)
		{
			return await this._db.StringGetAsync(this._keyprefix + key);
		}

		public async Task<bool> SetAsync(string key, string val)
		{
			return await this._db.StringSetAsync(this._keyprefix + key, val);
		}


		public async Task AddMRUAsync(string bid)
		{
			string key = this._keyprefix + "recent/";
			await this._db.SortedSetAddAsync(key, bid, DateTime.Now.Ticks);
		}
		public async Task RemoveMRUAsync(string bid)
		{
			string key = this._keyprefix + "recent/";
			await this._db.SortedSetRemoveAsync(key, bid);
		}


		public async Task<string[]> GetMRUAsync(long count)
		{
			string key = this._keyprefix + "recent/";
			var val = await this._db.SortedSetRangeByRankAsync(key, 0, count - 1, Order.Descending);
			return val.ToStringArray();
		}

		public async Task ClearCacheAsync()
		{

			var endpoints = this._conn.GetEndPoints();
			foreach (var ep in endpoints)
			{
				var server = this._conn.GetServer(ep);
				await server.FlushAllDatabasesAsync();
			}
		}
	}


	public class RedisService
	{
		private readonly ILogger _logger;
		private RedisWrapper db;
		private readonly string _dbname;

		public RedisService(SiteSettings settings, ILogger logger)
		{
			this._logger = logger;
			logger.WriteInformation("Starting Redis");
			this.db = new RedisWrapper(settings);
		}


		public async Task AddRecentlyModifiedAsync(string bid)
		{
			await this.db.AddMRUAsync(bid);
		}

		public async Task RemoveRecentlyModifiedAsync(string bid)
		{
			await this.db.RemoveMRUAsync(bid);
		}

		public async Task<string[]> GetRecentlyModifiedAsync(long count = 100)
		{
			return await this.db.GetMRUAsync(count);
		}


		public async Task ForgetAsync(string key)
		{
			await this.db.ForgetAsync(key);
		}

		public async Task ClearCacheAsync()
		{
			await this.db.ClearCacheAsync();
		}

		public async Task<bool> CacheBuildAsync(Build b)
		{
			this._logger.WriteInformation("Redis caching build " + b.id);
			var json = JsonConvert.SerializeObject(b);
			return await this.db.SetAsync(b.id, json);
		}
		public async Task<bool> CacheListAsync(BuildList list)
		{
			this._logger.WriteInformation("Redis caching list " + list.id);
			var json = JsonConvert.SerializeObject(list);
			return await this.db.SetAsync(list.id, json);
		}

		public async Task<Build> GetBuildAsync(string id)
		{
			var json = await this.db.GetAsync(id);
			if (json == null) return null;

			Build b = JsonConvert.DeserializeObject<Build>(json);
			if (b.DocType == "build")
			{
				this._logger.WriteInformation("Redis Found Build: " + id);
				return b;
			}

			return null;
		}

		public async Task<BuildList> GetListAsync(string id)
		{
			var json = await this.db.GetAsync(id);
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
			var json = await this.db.GetAsync("userbuilds/" + id);
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
			var json = await this.db.GetAsync("userlists/" + id);
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
			return await this.db.SetAsync("userbuilds/" + uid, json);
		}
		public async Task<bool> CacheUserListsAsync(string uid, BuildList[] list)
		{
			this._logger.WriteInformation("Redis caching user lists for " + uid);
			var json = JsonConvert.SerializeObject(list);
			return await this.db.SetAsync("userlists/" + uid, json);
		}


	}
}