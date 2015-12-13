namespace GearUp.Controllers
{
	using GearUp.Interfaces;
	using GearUp.Models;
	using Microsoft.AspNet.Mvc;
	using Microsoft.Extensions.Logging;
	using System.Collections.Generic;
	using System.Threading.Tasks;

	[Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly ILogger _logger;
        private readonly IAppDataService _ddb;

        public UserController(IAppDataService ddb, ILogger logger)
        {
            this._ddb = ddb;
            this._logger = logger;
        }

        [Produces("application/json", "text/json")]
        [HttpGet("builds/{id}")]
        public async Task<Build[]> UserBuilds(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new Build[0];
            }
            var b = await this._ddb.GetUserBuildsAsync(id);

            return b;
        }

        [Produces("application/json", "text/json")]
        [HttpGet("lists/{id}")]
        public async Task<IEnumerable<BuildList>> UserLists(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new BuildList[0];
            }
            var b = await this._ddb.GetUserListsAsync(id);

            return b;
        }
    }
}
