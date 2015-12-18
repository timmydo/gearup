namespace GearUp.Controllers
{
	using GearUp.Interfaces;
	using GearUp.Models;
	using Microsoft.AspNet.Mvc;
	using Microsoft.Extensions.Logging;
	using Shared.Interfaces;
	using System.Collections.Generic;
	using System.Threading.Tasks;

	[Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly ILogger _logger;
        private readonly IPartitionedKeyValueDictionary _ddb;
		public readonly string UserBuildNamespace = "ub/";
		public readonly string UserListNamespace = "ul/";

        public UserController(IPartitionedKeyValueDictionary ddb, ILogger logger)
        {
            this._ddb = ddb;
            this._logger = logger;
        }

        [Produces("application/json", "text/json")]
        [HttpGet("builds/{id}")]
        public async Task<string> UserBuilds(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return "[]";
            }

            return await this._ddb.GetKeyAsync(UserBuildNamespace + id);
        }

        [Produces("application/json", "text/json")]
        [HttpGet("lists/{id}")]
        public async Task<string> UserLists(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
				return "[]";
            }

            return await this._ddb.GetKeyAsync(UserListNamespace + id);
        }
    }
}
