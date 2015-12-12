using System.Threading.Tasks;
using GearUp.Models;


namespace GearUp.Interfaces
{
    public interface IAppCache : IAppBaseStorage
    {
		Task ForgetAsync(string key);

        Task AddRecentlyModifiedAsync(string bid);
        Task RemoveRecentlyModifiedAsync(string bid);
        Task SetRecentlyModifiedAsync(string[] items);
        Task ClearCacheAsync();

        Task CacheBuildAsync(Build b);
        Task CacheListAsync(BuildList list);
        Task CacheUserBuildsAsync(string uid, Build[] list);
        Task CacheUserListsAsync(string uid, BuildList[] list);

    }

}