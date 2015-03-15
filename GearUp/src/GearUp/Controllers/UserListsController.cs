

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

using Microsoft.AspNet.Mvc;
using Microsoft.Framework.Logging;
using GearUp.Models;
using System.Threading.Tasks;
using GearUp.Services;
using System.Security.Claims;
using System.Collections.Generic;

namespace GearUp.Controllers
{
	[Route("api/[controller]")]
	public class UserListsController : Controller
    {

		private readonly ILogger _logger;
		private readonly DocumentDB _ddb;

		public UserListsController(DocumentDB ddb, ILogger logger)
		{
			this._ddb = ddb;
			this._logger = logger;
		}


		[Produces("application/json", "text/json")]
		[HttpGet("{id}")]
		public async Task<IEnumerable<BuildList>> GetById(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				return new BuildList[0];
			}
			var b = await this._ddb.GetUserLists(id);

			return b;
		}


	}
}
