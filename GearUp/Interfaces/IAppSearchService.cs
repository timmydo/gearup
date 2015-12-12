using System.Threading.Tasks;
using GearUp.Models;

namespace GearUp.Interfaces
{
    public interface IAppSearchService
    {
		// Ensure indices have been created
		Task Startup();
		
		Task<Build[]> SearchBuilds(string query);
		
		

    }

}