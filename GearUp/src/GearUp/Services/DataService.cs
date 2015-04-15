using Microsoft.Framework.Logging;
using System;
using GearUp.Models;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using System.Collections.Generic;
using System.Linq;

namespace GearUp.Services
{
    public class DataService
    {

		private readonly RedisService redis;
		private readonly DocumentDB ddb;
		private readonly ILogger logger;
		public DataService(DocumentDB ddb, RedisService redis, ILogger logger)
		{
			this.ddb = ddb;
			this.redis = redis;
			this.logger = logger;
		}


		public async Task<string[]> GetRecentlyBuildsAsync()
		{
			return await this.redis.GetRecentlyModifiedAsync();
		}

		public async Task<Build> GetBuildAsync(string id)
		{
			Build b = await this.redis.GetBuildAsync(id);
			if (b == null)
			{
				b = await this.ddb.GetBuildAsync(id);
				if (b != null)
				{
					await this.redis.CacheBuildAsync(b);
				}

				return b;
			}
			else
			{
				return b;
			}
		}


		public async Task<BuildList> GetListAsync(string id)
		{
			BuildList list = await this.redis.GetListAsync(id);
			if (list == null)
			{
				list = await this.ddb.GetListAsync(id);
				if (list != null)
				{
					await this.redis.CacheListAsync(list);
				}

				return list;
			}
			else
			{
				return list;
			}
		}

		

		public async Task<Document> CreateBuildAsync(Build item)
		{
			await this.redis.ForgetAsync("userbuilds/" + item.Creator);
			return await this.ddb.CreateBuildAsync(item);
		}

		public async Task<Document> CreateListAsync(BuildList item)
		{
			await this.redis.ForgetAsync("userlists/" + item.Creator);
			return await this.ddb.CreateListAsync(item);
		}


		public async Task AddImageToBuildAsync(string buildGuid, string imageGuid, string uid)
		{
			await this.redis.ForgetAsync(buildGuid);
			await this.ddb.AddImageToBuildAsync(buildGuid, imageGuid, uid);
        }

		public async Task AddBuildToListAsync(string buildGuid, string listGuid, string uid)
		{
			await this.redis.ForgetAsync(listGuid);
			await this.ddb.AddBuildToListAsync(buildGuid, listGuid, uid);
		}
		public async Task RemoveBuildFromListAsync(string buildGuid, string listGuid, string uid)
		{
			await this.redis.ForgetAsync(listGuid);
			await this.ddb.RemoveBuildFromListAsync(buildGuid, listGuid, uid);

		}

		public async Task DeleteImageFromBuildAsync(string buildGuid, string imageGuid, string uid)
		{
			await this.redis.ForgetAsync(buildGuid);
			await this.ddb.DeleteImageFromBuildAsync(buildGuid, imageGuid, uid);
		}

		public async Task<string> SaveBuildAsync(Build b, string uid)
		{
			await this.redis.ForgetAsync(b.id);
			await this.redis.AddRecentlyModifiedAsync(b.id);
			return await this.ddb.SaveBuildAsync(b, uid);
		}

		public async Task<string> SaveListAsync(BuildList l, string uid)
		{
			await this.redis.ForgetAsync(l.id);
			return await this.ddb.SaveListAsync(l, uid);
		}


		public async Task DeleteBuildAsync(Build b, string uid)
		{
			await this.redis.ForgetAsync("userbuilds/" + b.Creator);
			await this.ddb.DeleteBuildAsync(b, uid);
			await this.redis.ForgetAsync(b.id);
		}

		public async Task DeleteListAsync(BuildList b, string uid)
		{
			await this.redis.ForgetAsync("userlists/" + b.Creator);
			await this.ddb.DeleteListAsync(b, uid);
			await this.redis.ForgetAsync(b.id);
		}



		public async Task<Build[]> GetUserBuilds(string id)
		{
			Build[] builds = await this.redis.GetUserBuildsAsync(id);
			if (builds != null)
			{
				return builds;
			}

			var res = await this.ddb.GetUserBuilds(id);
			builds = res.ToArray<Build>();
			await this.redis.CacheUserBuildsAsync(id, builds);
			return builds;
		}
		public async Task<IEnumerable<BuildList>> GetUserLists(string id)
		{
			BuildList[] lists = await this.redis.GetUserListsAsync(id);
			if (lists != null)
			{
				return lists;
			}

			var res = await this.ddb.GetUserLists(id);
			lists = res.ToArray<BuildList>();
			await this.redis.CacheUserListsAsync(id, lists);
			return lists;

		}

	}
}