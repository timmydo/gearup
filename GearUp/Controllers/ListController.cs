
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
		private IUserAuthenticator _ua;

		public readonly string ListNamespace = "l/";

		public ListController(IPartitionedKeyValueDictionary ddb, ILogger logger, IUserAuthenticator ua)
		{
			this._data = ddb;
			this._logger = logger;
			this._ua = ua;
		}

		[Produces("application/json", "text/json")]
		[HttpGet("get/{id}")]
		public async Task<BuildList> GetById(string id)
		{
			var blstr = await this._data.GetKeyAsync(ListNamespace + id);
			if (string.IsNullOrEmpty(blstr))
			{
				HttpContext.Response.StatusCode = 404;
				return null;
			}

			var bl = JsonConvert.DeserializeObject<BuildList>(blstr);
			return bl;
		}

		[Produces("application/json", "text/json")]
		[HttpPost("get")]
		public async Task<List<BuildList>> GetMultiple([FromBody]List<string> bl)
		{
			if (bl == null)
			{
				HttpContext.Response.StatusCode = 400;
				return null;
			}

			this._logger.LogInformation("Get lists, count: " + bl.Count);
			if (bl.Count < 1)
			{
				return new List<BuildList>();
			}
			var list = new List<BuildList>();
			foreach (var id in bl)
			{
				var fullBuildList = await this._data.GetKeyAsync(ListNamespace + id);
				if (!string.IsNullOrEmpty(fullBuildList))
				{
					list.Add(JsonConvert.DeserializeObject<BuildList>(fullBuildList));
				}
			}

			return list;
		}


		[HttpPost("create")]
		public async Task<BuildList> CreateList()
		{
			var uid = _ua.AuthenticateUser(this);
			if (uid == null)
			{
				HttpContext.Response.StatusCode = 401;
				return null;
			}

			var bl = new BuildList()
			{
				Id = Guid.NewGuid().ToString("N"),
				Creator = uid.UserId,
				Title = string.Empty,
				Description = string.Empty,
				Builds = new List<string>(),
				Created = DateTime.UtcNow,
				Modified = DateTime.UtcNow
			};

			await this._data.AddKeyAsync(ListNamespace + bl.Id, JsonConvert.SerializeObject(bl));

			await ListHelper.AddToList(_data, UserController.UserListNamespace, uid.UserId, bl.Id);

			return bl;
		}

		[HttpPost("delete")]
		public async Task<string> DeleteList([FromBody]string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				HttpContext.Response.StatusCode = 400;
				return "invalid list";
			}
			var uid = _ua.AuthenticateUser(this);

			if (uid == null)
			{
				HttpContext.Response.StatusCode = 401;
				return "log in";
			}

			var ldata = await this._data.GetKeyAsync(ListNamespace + id);
			if (string.IsNullOrEmpty(ldata))
			{
				HttpContext.Response.StatusCode = 404;
				return "not found";
			}

			var list = JsonConvert.DeserializeObject<BuildList>(ldata);
			if (list.Creator != uid.UserId)
			{
				HttpContext.Response.StatusCode = 403;
				return "must be owner to edit list";
			}


			await this._data.DeleteKeyAsync(ListNamespace + id);
			await ListHelper.RemoveFromList(_data, UserController.UserListNamespace, uid.UserId, list.Id);

			return "Deleted";
		}

		[HttpPost("save")]
		public async Task<string> Save([FromBody]BuildList bl)
		{
			var uid = _ua.AuthenticateUser(this);

			if (uid == null)
			{
				HttpContext.Response.StatusCode = 401;
				return "log in first";
			}

			if (bl == null)
			{
				HttpContext.Response.StatusCode = 400;
				return "invalid build list";
			}

			var ldata = await this._data.GetKeyAsync(ListNamespace + bl.Id);

			if (string.IsNullOrEmpty(ldata))
			{
				HttpContext.Response.StatusCode = 404;
				return "not found";
			}

			var actualList = JsonConvert.DeserializeObject<BuildList>(ldata);
			if (actualList.Creator != uid.UserId)
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

			if (await this._data.UpdateKeyAsync(ListNamespace + bl.Id, JsonConvert.SerializeObject(bl)))
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
