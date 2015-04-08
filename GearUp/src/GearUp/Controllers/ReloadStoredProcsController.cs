

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
	public class ReloadStoredProcsController : Controller
    {

		private readonly ILogger _logger;
		private readonly DocumentDB _ddb;

		public ReloadStoredProcsController(DocumentDB ddb, ILogger logger)
		{
			this._ddb = ddb;
			this._logger = logger;
		}


		[HttpGet()]
		public async Task<string> Get()
		{
			this._logger.WriteInformation("Reloading stored procs");
			await this._ddb.LoadStoredProcs(true);
			return "Done";
		}

	}
}
