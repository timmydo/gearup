namespace GearUp.Services
{
	using System;
	using GearUp.Models;
	using System.Threading.Tasks;
	using System.Collections.Generic;
	using System.Linq;
	using Microsoft.Extensions.Logging;

	public class DataService
    {
		private readonly ILogger logger;

		public DataService(ILogger logger)
		{
			this.logger = logger;
		}

		public async Task<string[]> GetRecentBuildsAsync()
		{
			throw new NotImplementedException();
		}

		public async Task<Build> GetBuildAsync(string id)
		{
			throw new NotImplementedException();
		}

		public async Task<BuildList> GetListAsync(string id)
		{
			throw new NotImplementedException();
		}

		public async Task CreateBuildAsync(Build item)
		{
			throw new NotImplementedException();
		}

		public async Task CreateListAsync(BuildList item)
		{
			throw new NotImplementedException();
		}

		public async Task AddImageToBuildAsync(string buildGuid, string imageGuid, string uid)
		{
			throw new NotImplementedException();
		}

		public async Task AddBuildToListAsync(string buildGuid, string listGuid, string uid)
		{
			throw new NotImplementedException();
		}
		public async Task RemoveBuildFromListAsync(string buildGuid, string listGuid, string uid)
		{
			throw new NotImplementedException();
		}

		public async Task DeleteImageFromBuildAsync(string buildGuid, string imageGuid, string uid)
		{
			throw new NotImplementedException();
		}

		public async Task<string> SaveBuildAsync(Build b, string uid)
		{
			throw new NotImplementedException();
		}

		public async Task<string> SaveListAsync(BuildList l, string uid)
		{
			throw new NotImplementedException();
		}


		public async Task DeleteBuildAsync(Build b, string uid)
		{
			throw new NotImplementedException();
		}

		public async Task DeleteListAsync(BuildList b, string uid)
		{
			throw new NotImplementedException();
		}

		public async Task<Build[]> GetUserBuildsAsync(string id)
		{
			throw new NotImplementedException();
		}

		public async Task<IEnumerable<BuildList>> GetUserListsAsync(string id)
		{
			throw new NotImplementedException();
		}

	}
}