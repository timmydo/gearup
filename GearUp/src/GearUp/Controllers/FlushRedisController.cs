

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
	public class ClearRedisController : Controller
    {

		private readonly ILogger _logger;
		private readonly RedisService _redis;

		public ClearRedisController(RedisService ddb, ILogger logger)
		{
			this._redis = ddb;
			this._logger = logger;
		}


		[HttpGet()]
		public async Task<string> Get()
		{
			this._logger.WriteInformation("Flushing Redis Cache");
			await this._redis.ClearCacheAsync();
			return "Done";
		}

	}
}
