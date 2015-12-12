namespace GearUp.Controllers.Controllers
{
	using System;
	using System.Threading.Tasks;
	using Microsoft.AspNet.Mvc;
	using GearUp.Services;
	using Microsoft.Extensions.Logging;

	[Route("api/[controller]")]
	public class DeleteImageFromBuildController : Controller
	{
		private readonly ILogger _logger;
		private readonly DataService _ddb;

		public DeleteImageFromBuildController(SiteSettings settings, ILogger logger, DataService ddb)
		{
			this._logger = logger;
			this._ddb = ddb;
		}

		public class ParamInfo
		{
			public string Build { get; set; }
			public string Image { get; set; }

		}

		// POST api/values
		[HttpPost]
		public async Task<string> Post([FromBody]ParamInfo pi)
		{
			var uid = UserLogin.UserUniqueId(User.Identity);

			if (string.IsNullOrEmpty(uid))
			{
				throw new Exception("User is not logged in");
			}

			await this._ddb.DeleteImageFromBuildAsync(pi.Build, pi.Image, uid);

			return "Success";
		}

	}
}
