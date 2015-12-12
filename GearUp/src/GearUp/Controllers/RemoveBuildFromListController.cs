namespace GearUp.Controllers.Controllers
{
	using System;
	using System.Threading.Tasks;
	using Microsoft.AspNet.Mvc;
	using GearUp.Services;
	using Microsoft.Extensions.Logging;

	[Route("api/[controller]")]
	public class RemoveBuildFromListController : Controller
	{


		private readonly ILogger _logger;
		private readonly DataService _data;

		public RemoveBuildFromListController(SiteSettings settings, ILogger logger, DataService ddb)
		{
			this._logger = logger;
			this._data = ddb;
		}

		public class ParamInfo
		{
			public string Build { get; set; }
			public string List { get; set; }

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

			var b = await this._data.GetBuildAsync(pi.Build);
			var l = await this._data.GetListAsync(pi.List);

			await this._data.RemoveBuildFromListAsync(b.id, l.id, uid);

			return "Success";
		}

	}
}
