namespace GearUp.Controllers
{
	using GearUp.Models;
	using GearUp.Services;
	using Microsoft.AspNet.Mvc;
	using Microsoft.Extensions.Logging;
	using System.Collections.Generic;
	using System.Threading.Tasks;

	[Route("api/[controller]")]
	public class UserListsController : Controller
    {
		private readonly ILogger _logger;
		private readonly DataService _ddb;

		public UserListsController(DataService ddb, ILogger logger)
		{
			this._ddb = ddb;
			this._logger = logger;
		}

		[Produces("application/json", "text/json")]
		[HttpGet("{id}")]
		public async Task<IEnumerable<BuildList>> GetById(string id)
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
