
namespace GearUp.Controllers
{
	using GearUp.Interfaces;
	using GearUp.Models;
	using Microsoft.AspNet.Mvc;
	using Microsoft.Extensions.Logging;
	using System.Threading.Tasks;

	[Route("api/[controller]")]
    public class SearchController : Controller
    {

        private readonly ILogger _logger;
        private readonly IAppSearchService _search;

        public SearchController(IAppSearchService ss, ILogger logger)
        {
            this._search = ss;
            this._logger = logger;
        }

		[Produces("application/json", "text/json")]
		[HttpGet("builds/{str}")]
		public async Task<Build[]> SearchBuilds(string str)
		{
			var b = await this._search.SearchBuildsAsync(str);
			return b;
		}
	}
}
