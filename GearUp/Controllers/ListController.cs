
namespace GearUp.Controllers
{
	using GearUp.Auth;
	using GearUp.Interfaces;
	using GearUp.Models;
	using Microsoft.AspNet.Mvc;
	using Microsoft.Extensions.Logging;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	using Shared.Interfaces;
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;

	[Route("api/[controller]")]
	public class ListController : Controller
	{

		private readonly ILogger _logger;
		private readonly IPartitionedKeyValueDictionary _data;

		public readonly string ListNamespace = "l/";

		public ListController(IPartitionedKeyValueDictionary ddb, ILogger logger)
		{
			this._data = ddb;
			this._logger = logger;
		}

		[Produces("application/json", "text/json")]
		[HttpGet("get/{id}")]
		public async Task<BuildList> GetById(string id)
		{
			var blstr = await this._data.GetKeyAsync(ListNamespace + id);
			var bl = JsonConvert.DeserializeObject<BuildList>(blstr);
			return bl;
		}

		[HttpPost("create")]
		public async Task<BuildList> CreateList()
		{
			var uid = UserLogin.UserUniqueId(User?.Identity);
			if (string.IsNullOrEmpty(uid))
			{
				HttpContext.Response.StatusCode = 401;
				return null;
			}

			var bl = new BuildList()
			{
				Id = Guid.NewGuid().ToString("N"),
				Creator = uid,
				Title = string.Empty,
				Description = string.Empty,
				Builds = new List<string>(),
				Created = DateTime.UtcNow,
				Modified = DateTime.UtcNow
			};

			await this._data.AddKeyAsync(ListNamespace + bl.Id, JsonConvert.SerializeObject(bl));

			return bl;
		}

		[HttpPost("delete")]
		public async Task<string> Delete([FromBody]string id)
		{
			if (!string.IsNullOrEmpty(id))
			{
				var uid = UserLogin.UserUniqueId(User?.Identity);

				var ldata = await this._data.GetKeyAsync(ListNamespace + id);
				if (string.IsNullOrEmpty(ldata))
				{
					HttpContext.Response.StatusCode = 404;
					return "not found";
				}

				var list = JsonConvert.DeserializeObject<BuildList>(ldata);
				if (list.Creator != uid)
				{
					HttpContext.Response.StatusCode = 403;
					return "must be owner to edit list";
				}


				await this._data.DeleteKeyAsync(ListNamespace + id);
				return "Deleted";
			}
			else
			{
				HttpContext.Response.StatusCode = 400;
				return "invalid list";
			}
		}

		[HttpPost("save")]
		public async Task<string> Save([FromBody]BuildList bl)
		{
			var uid = UserLogin.UserUniqueId(User?.Identity);

			if (string.IsNullOrEmpty(uid))
			{
				HttpContext.Response.StatusCode = 401;
				return "log in first";
			}

			var ldata = await this._data.GetKeyAsync(ListNamespace + bl.Id);

			if (string.IsNullOrEmpty(ldata))
			{
				HttpContext.Response.StatusCode = 404;
				return "not found";
			}

			var actualList = JsonConvert.DeserializeObject<BuildList>(ldata);
			if (actualList.Creator != uid)
			{
				HttpContext.Response.StatusCode = 403;
				return "must be owner to edit list";
			}

			// do not accept updates from these fields
			bl.Creator = actualList.Creator;
			bl.Created = actualList.Created;

			if (bl.Builds == null)
			{
				bl.Builds = new List<string>();
			}

			if (await this._data.UpdateKeyAsync(ListNamespace + bl.Id, JsonConvert.SerializeObject(bl), "timestamp"))
			{
				return "success";
			}
			else
			{
				HttpContext.Response.StatusCode = 500;
				return "error updating";
			}
		}

	}
}
