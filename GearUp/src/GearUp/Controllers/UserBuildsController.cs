﻿

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
	public class UserBuildsController : Controller
    {

		private readonly ILogger _logger;
		private readonly DataService _ddb;

		public UserBuildsController(DataService ddb, ILogger logger)
		{
			this._ddb = ddb;
			this._logger = logger;
		}


		[Produces("application/json", "text/json")]
		[HttpGet("{id}")]
		public async Task<Build[]> GetById(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				return new Build[0];
			}
			var b = await this._ddb.GetUserBuilds(id);

			return b;
		}


	}
}
