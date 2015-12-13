using System.Threading.Tasks;
using GearUp.Models;
using System.Collections.Generic;


namespace GearUp.Interfaces
{
    public interface IAppDataService
    {
		 Task<string[]> GetRecentBuildsAsync();

		 Task<Build> GetBuildAsync(string id);
		 Task<BuildList> GetListAsync(string id);
		 Task CreateBuildAsync(Build item);
		 Task CreateListAsync(BuildList item);

		 Task AddImageToBuildAsync(string buildGuid, string imageGuid, string uid);
		 Task AddBuildToListAsync(string buildGuid, string listGuid, string uid);
		 Task RemoveBuildFromListAsync(string buildGuid, string listGuid, string uid);

		 Task DeleteImageFromBuildAsync(string buildGuid, string imageGuid, string uid);
		 Task<string> SaveBuildAsync(Build b, string uid);

		 Task<string> SaveListAsync(BuildList l, string uid);


		 Task DeleteBuildAsync(Build b, string uid);

		 Task DeleteListAsync(BuildList b, string uid);


		 Task<Build[]> GetUserBuilds(string id);
		 Task<IEnumerable<BuildList>> GetUserLists(string id);

    }

}