namespace GearUp.Controllers
{
	using GearUp.Services;
	using Microsoft.AspNet.Mvc;
	using Microsoft.Extensions.Logging;
	using System.Threading.Tasks;


	[Route("api/[controller]")]
	public class RecentBuildsController
    {
		private readonly ILogger _logger;
		private readonly DataService _data;

		public RecentBuildsController(DataService ddb, ILogger logger)
		{
			this._data = ddb;
			this._logger = logger;
		}

		[Produces("application/json", "text/json")]
		public async Task<string[]> Get()
		{
			return await this._data.GetRecentBuildsAsync();
		}



	}
}