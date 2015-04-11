

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
	public class ListController : Controller
    {

		private readonly ILogger _logger;
		private readonly DataService _ddb;

		public ListController(DataService ddb, ILogger logger)
		{
			this._ddb = ddb;
			this._logger = logger;
		}


		[Produces("application/json", "text/json")]
		[HttpGet("{id}")]
		public async Task<BuildList> GetById(string id)
		{
			var b = this._ddb.GetList(id);
			if (b == null)
			{
				b = new BuildList();
				b.id = id;
				var uid = UserLogin.UserUniqueId(User.Identity);
				b.Creator = uid;
				if (string.IsNullOrEmpty(uid))
				{
					b.Title = "Must log in to create lists";
					return b;
				}
				this._logger.WriteInformation("Setting creator to " + uid);
				await this._ddb.CreateListAsync(b);
			}
			return b;
		}


	}
}
