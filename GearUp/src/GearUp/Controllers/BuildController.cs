

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

using Microsoft.AspNet.Mvc;
using Microsoft.Framework.Logging;
using GearUp.Models;
using System.Threading.Tasks;
using GearUp.Services;
using System.Security.Claims;
using System;
using System.Collections.Generic;

namespace GearUp.Controllers
{
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
			var b = this._data.GetBuild(id);
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
				this._logger.WriteInformation("Setting creator to " + uid);
				await this._data.CreateBuildAsync(b);
			}
			return b;
		}

		public Build[] Post([FromBody]BuildList bl)
		{
			if (bl.Builds.Count < 1)
			{
				return new Build[0];
			}
			var list = new List<Build>();
			foreach (var id in bl.Builds)
			{
				var fullBuild = this._data.GetBuild(id);
				if (fullBuild != null)
				{
					list.Add(fullBuild);
				}
			}

			return list.ToArray();
		}


	}
}
