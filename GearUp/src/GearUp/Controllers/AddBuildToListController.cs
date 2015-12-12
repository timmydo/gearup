

namespace GearUp.Controllers.Controllers
{
	using System;
	using System.Threading.Tasks;
	using Microsoft.AspNet.Mvc;
	using GearUp.Services;
	using Microsoft.Extensions.Logging;

	[Route("api/[controller]")]
	public class AddBuildToListController : Controller
	{


		private readonly ILogger _logger;
		private readonly DataService _data;

		public AddBuildToListController(SiteSettings settings, ILogger logger, DataService data)
		{
			this._logger = logger;
			this._data = data;
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

			await this._data.AddBuildToListAsync(b.id, l.id, uid);

			return "Success";
		}

	}
}
