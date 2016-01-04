using System.Threading.Tasks;
using GearUp.Models;
using System.Collections.Generic;

namespace GearUp.Interfaces
{
    public interface IAppSearchService
    {
		Task<Build[]> SearchBuildsAsync(string query);

		Task AddBuildsToIndexAsync(IEnumerable<Build> b);
		Task RemoveBuildsFromIndexAsync(IEnumerable<Build> b);

    }

}