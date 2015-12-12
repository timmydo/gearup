namespace GearUp.Controllers
{
	using Microsoft.AspNet.Mvc;
	using GearUp.Models;
	using System.Threading.Tasks;
	using GearUp.Services;
	using Microsoft.Extensions.Logging;

	[Route("api/[controller]")]
	public class UserBuildsController : Controller
    {

		private readonly ILogger _logger;
		private readonly DataService _ddb;

		public UserBuildsController(DataService ddb, ILogger logger)
		{
			this._ddb = ddb;
			this._logger = logger;
		}

		[Produces("application/json", "text/json")]
		[HttpGet("{id}")]
		public async Task<Build[]> GetById(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				return new Build[0];
			}
			var b = await this._ddb.GetUserBuildsAsync(id);

			return b;
		}
	}
}
