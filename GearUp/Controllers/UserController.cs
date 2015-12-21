namespace GearUp.Controllers
{
	using GearUp.Interfaces;
	using GearUp.Models;
	using Microsoft.AspNet.Mvc;
	using Microsoft.Extensions.Logging;
	using Newtonsoft.Json;
	using Shared.Interfaces;
	using System.Collections.Generic;
	using System.Threading.Tasks;

	[Route("api/[controller]")]
	public class UserController : Controller
	{
		private readonly ILogger _logger;
		private readonly IPartitionedKeyValueDictionary _ddb;
		public static readonly string UserBuildNamespace = "ub/";
		public static readonly string UserListNamespace = "ul/";

		public UserController(IPartitionedKeyValueDictionary ddb, ILogger logger)
		{
			this._ddb = ddb;
			this._logger = logger;
		}

		private async Task<List<string>> GetList(string ns, string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				HttpContext.Response.StatusCode = 400;
				return null;
			}

			var data = await this._ddb.GetKeyAsync(ns + id);
			if (string.IsNullOrEmpty(data))
			{
				HttpContext.Response.StatusCode = 404;
				return null;
			}

			var list = JsonConvert.DeserializeObject<List<string>>(data);
			return list;
		}

		[Produces("application/json", "text/json")]
		[HttpGet("builds/{id}")]
		public async Task<List<string>> UserBuilds(string id)
		{
			return await GetList(UserBuildNamespace, id);
		}

		[Produces("application/json", "text/json")]
		[HttpGet("lists/{id}")]
		public async Task<List<string>> UserLists(string id)
		{
			return await GetList(UserListNamespace, id);
		}
	}
}
