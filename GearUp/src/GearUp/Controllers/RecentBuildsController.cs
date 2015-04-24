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
	public class RecentBuildsController
    {
		private readonly ILogger _logger;
		private readonly DataService _data;

		public RecentBuildsController(DataService ddb, ILogger logger)
		{
			this._data = ddb;
			this._logger = logger;
		}

		[Produces("application/json", "text/json")]
		public async Task<string[]> Get()
		{
			return await this._data.GetRecentBuildsAsync();
		}



	}
}