using System.Threading.Tasks;
using GearUp.Models;
using System.Collections.Generic;


namespace GearUp.Interfaces
{
    public interface IAppDatabase: IAppBaseStorage
    {
        Task LoadStoredProcs(bool reload);
        
		
		Task AddImageToBuildAsync(string buildGuid, string imageGuid, string uid);
        Task DeleteImageFromBuildAsync(string buildGuid, string imageGuid, string uid);
        
        Task CreateBuildAsync(Build item);
        Task<string> SaveBuildAsync(Build b, string uid);

        Task DeleteBuildAsync(Build b, string uid);


        Task CreateListAsync(BuildList item);
        Task<string> SaveListAsync(BuildList l, string uid);
        Task DeleteListAsync(BuildList b, string uid);
        Task RemoveBuildFromListAsync(string buildGuid, string listGuid, string uid);
        Task AddBuildToListAsync(string buildGuid, string listGuid, string uid);




    }

}