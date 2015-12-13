using System.Threading.Tasks;
using GearUp.Models;


namespace GearUp.Interfaces
{
    public interface IAppBaseStorage
    {


        Task<Build> GetBuildAsync(string id);
        Task<BuildList> GetListAsync(string id);
        Task<Build[]> GetUserBuildsAsync(string id);
        Task<BuildList[]> GetUserListsAsync(string id);
        Task<string[]> GetRecentlyModifiedAsync(long count);


    }

}