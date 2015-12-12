namespace GearUp.Controllers.Controllers
{
	using System;
	using System.Threading.Tasks;
	using Microsoft.AspNet.Mvc;
	using GearUp.Services;
	using GearUp.Models;
	using Microsoft.Extensions.Logging;

	[Route("api/[controller]")]
	public class SaveListController : Controller
	{


		private readonly ILogger _logger;
		private readonly DataService _ddb;

		public SaveListController(SiteSettings settings, ILogger logger, DataService ddb)
		{
			this._logger = logger;
			this._ddb = ddb;
		}


		// POST api/values
		[HttpPost]
		public async Task<string> Post([FromBody]BuildList b)
		{
			if (b != null && !string.IsNullOrEmpty(b.Creator))
			{
				var uid = UserLogin.UserUniqueId(User.Identity);
				var newId = await this._ddb.SaveListAsync(b, uid);
                return newId;
			}
			else
			{
				throw new Exception("Invalid List");
			}
		}

	}
}
