namespace GearUp.Controllers.Controllers
{
	using System;
	using System.Threading.Tasks;
	using Microsoft.AspNet.Mvc;
	using GearUp.Services;
	using GearUp.Models;
	using Microsoft.Extensions.Logging;


	[Route("api/[controller]")]
	public class SaveBuildController : Controller
	{


		private readonly ILogger _logger;
		private readonly DataService _data;

		public SaveBuildController(SiteSettings settings, ILogger logger, DataService ddb)
		{
			this._logger = logger;
			this._data = ddb;
			
		}


		// POST api/values
		[HttpPost]
		public async Task<string> Post([FromBody]Build b)
		{
			if (User == null)
			{
				throw new Exception("User is null");
			}

			if (b != null && !string.IsNullOrEmpty(b.Creator))
			{
				var uid = UserLogin.UserUniqueId(User.Identity);
				this._logger.LogInformation("SaveBuild Controller Post");
				var newId = await this._data.SaveBuildAsync(b, uid);
                return newId;
			}
			else
			{
				throw new Exception("Invalid Build");
			}
		}
	}
}
