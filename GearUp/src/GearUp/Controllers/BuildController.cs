namespace GearUp.Controllers
{
	using Microsoft.AspNet.Mvc;
	using GearUp.Models;
	using System.Threading.Tasks;
	using GearUp.Services;
	using System.Collections.Generic;
	using Microsoft.Extensions.Logging;

	[Route("api/[controller]")]
	public class BuildController : Controller
	{

		private readonly ILogger _logger;
		private readonly DataService _data;

		public BuildController(DataService ddb, ILogger logger)
		{
			this._data = ddb;
			this._logger = logger;
		}


		[Produces("application/json", "text/json")]
		[HttpGet("{id}")]
		public async Task<Build> GetById(string id)
		{
			var b = await this._data.GetBuildAsync(id);
			if (b == null)
			{
				b = new Build();
				b.id = id;
				var uid = UserLogin.UserUniqueId(User.Identity);
				b.Creator = uid;
				if (string.IsNullOrEmpty(uid))
				{
					b.Title = "Must log in to create builds";
					return b;
				}
				this._logger.LogInformation("Setting creator to " + uid);
				await this._data.CreateBuildAsync(b);
			}
			return b;
		}

		public async Task<Build[]> Post([FromBody]BuildList bl)
		{
			if (bl.Builds.Count < 1)
			{
				return new Build[0];
			}
			var list = new List<Build>();
			foreach (var id in bl.Builds)
			{
				var fullBuild = await this._data.GetBuildAsync(id);
				if (fullBuild != null)
				{
					list.Add(fullBuild);
				}
			}

			return list.ToArray();
		}


	}
}
