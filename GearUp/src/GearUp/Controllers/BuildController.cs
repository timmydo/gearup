

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

using Microsoft.AspNet.Mvc;
using Microsoft.Framework.Logging;
using GearUp.Models;
using System.Threading.Tasks;
using GearUp.Services;
using System.Security.Claims;
using System;

namespace GearUp.Controllers
{
	[Route("api/[controller]")]
	public class BuildController : Controller
    {

		private readonly ILogger _logger;
		private readonly DocumentDB _ddb;

		public BuildController(DocumentDB ddb, ILogger logger)
		{
			this._ddb = ddb;
			this._logger = logger;
		}


		[Produces("application/json", "text/json")]
		[HttpGet("{id}")]
		public async Task<Build> GetById(string id)
		{
			var b = this._ddb.GetBuild(id);
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
				await this._ddb.CreateBuildAsync(b);
			}
			return b;
		}


	}
}
