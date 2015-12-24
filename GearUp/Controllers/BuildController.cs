namespace GearUp.Controllers
{
	using GearUp.Auth;
	using GearUp.Interfaces;
	using GearUp.Models;
	using Microsoft.AspNet.Mvc;
	using Microsoft.Extensions.Logging;
	using Newtonsoft.Json;
	using Shared.Interfaces;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;


	[Route("api/[controller]")]
	public class BuildController : Controller
	{

		private readonly ILogger _logger;
		private readonly IPartitionedKeyValueDictionary _data;
		private readonly IAppBlobStorage _blobService;
		public readonly List<string> ValidContentTypes = new List<string>() { "image/png", "image/jpeg", "image/gif" };
		private IUserAuthenticator _ua;

		public readonly static string BuildNamespace = "b/";
		public readonly static string RecentBuildNamespace = "rb/";
		public readonly static string RecentBuildName = "*";

		public class DeleteImageParamInfo
		{
			public string Build { get; set; }
			public string Image { get; set; }

		}


		public BuildController(ILogger logger, IPartitionedKeyValueDictionary data, IAppBlobStorage blobService, IUserAuthenticator ua)
		{
			this._logger = logger;
			this._data = data;
			this._blobService = blobService;
			this._ua = ua;
		}


		[Produces("application/json", "text/json")]
		[HttpGet("get/{id}")]
		public async Task<Build> GetById(string id)
		{
			var b = await this._data.GetKeyAsync(BuildNamespace + id);
			if (string.IsNullOrEmpty(b))
			{
				HttpContext.Response.StatusCode = 404;
				return null;
			}
			var build = JsonConvert.DeserializeObject<Build>(b);
			return build;
		}


		[HttpPost("create")]
		public async Task<Build> CreateBuild()
		{
			var uid = _ua.AuthenticateUser(this);

			if (uid == null)
			{
				HttpContext.Response.StatusCode = 401;
				return null;
			}

			var b = new Build
			{
				Id = System.Guid.NewGuid().ToString("N"),
				Creator = uid.UserId,
				Created = DateTime.UtcNow,
				Modified = DateTime.UtcNow,
				Parts = new List<Part>(),
				Images = new List<Image>(),
				Title = string.Empty,
				Description = string.Empty
			};

			var bstr = JsonConvert.SerializeObject(b);
			await this._data.AddKeyAsync(BuildNamespace + b.Id, bstr);

			await ListHelper.AddToList(_data, UserController.UserBuildNamespace, uid.UserId, b.Id);
			await ListHelper.EnqueueList(_data, BuildController.RecentBuildNamespace, BuildController.RecentBuildName, b.Id);

			return b;
		}

		[HttpPost("delete")]
		public async Task<string> DeleteBuild([FromBody]string b)
		{
			if (string.IsNullOrEmpty(b))
			{
				HttpContext.Response.StatusCode = 400;
				return "bad build";
			}

			var uid = _ua.AuthenticateUser(this);
			var bdata = await this._data.GetKeyAsync(BuildNamespace + b);
			if (string.IsNullOrEmpty(bdata))
			{
				HttpContext.Response.StatusCode = 404;
				return "not found";
			}

			var build = JsonConvert.DeserializeObject<Build>(await this._data.GetKeyAsync(BuildNamespace + b));
			if (uid.UserId != build.Creator)
			{
				HttpContext.Response.StatusCode = 403;
				return "you are not the owner";
			}

			await this._data.DeleteKeyAsync(BuildNamespace + build.Id);
			await ListHelper.RemoveFromList(_data, UserController.UserBuildNamespace, uid.UserId, build.Id);
			await ListHelper.RemoveFromQueue(_data, BuildController.RecentBuildNamespace, BuildController.RecentBuildName, build.Id);

			return "Deleted";

		}


		[Produces("application/json", "text/json")]
		[HttpPost("get")]
		public async Task<List<Build>> GetMultiple([FromBody]List<string> bl)
		{
			if (bl == null)
			{
				HttpContext.Response.StatusCode = 400;
				return null;
			}

			this._logger.LogInformation("Get builds, count: " + bl.Count);
			if (bl.Count < 1)
			{
				return new List<Build>();
			}
			var list = new List<Build>();
			foreach (var id in bl)
			{
				var fullBuild = await this._data.GetKeyAsync(BuildNamespace + id);
				if (!string.IsNullOrEmpty(fullBuild))
				{
					list.Add(JsonConvert.DeserializeObject<Build>(fullBuild));
				}
			}

			return list;
		}

		[HttpPost("add-image")]
		public async Task<string> AddImage([FromQuery]string buildid)
		{
			var stream = Request.Body;
			var uid = _ua.AuthenticateUser(this);

			if (uid == null)
			{
				HttpContext.Response.StatusCode = 401;
				return "User is not logged in";
			}

			if (!ValidContentTypes.Contains(Request.ContentType))
			{
				HttpContext.Response.StatusCode = 400;
				return "Invalid Content Type";
			}

			if (string.IsNullOrEmpty(buildid))
			{
				HttpContext.Response.StatusCode = 404;
				return "Invalid Build ID";
			}

			var bdata = await this._data.GetKeyAsync(BuildNamespace + buildid);
			if (string.IsNullOrEmpty(bdata))
			{
				HttpContext.Response.StatusCode = 404;
				return "build not found";
			}

			var build = JsonConvert.DeserializeObject<Build>(bdata);

			if (build.Creator != uid.UserId)
			{
				HttpContext.Response.StatusCode = 403;
				return "cannot modify other user's build";
			}

			_logger.LogInformation("Upload Image, Content Type: " + Request.ContentType + " Build ID: " + buildid);

			var imageGuid = await this._blobService.UploadUserImage(stream, Request.ContentType);

			if (build.Images == null)
			{
				build.Images = new List<Image>();
			}

			build.Images.Add(new Image { Id = imageGuid });

			var success = await this._data.UpdateKeyAsync(BuildNamespace + buildid, JsonConvert.SerializeObject(build));

			if (!success)
			{
				//FIXME, try again?
				HttpContext.Response.StatusCode = 500;
				await this._blobService.DeleteImage(imageGuid);
				return "cannot update build";
			}

			return imageGuid;
		}

		[HttpPost("delete-image")]
		public async Task<string> DeleteImage([FromBody]DeleteImageParamInfo pi)
		{
			var uid = _ua.AuthenticateUser(this).UserId;

			if (string.IsNullOrEmpty(uid))
			{
				HttpContext.Response.StatusCode = 401;
				return "User is not logged in";
			}

			var bstr = await this._data.GetKeyAsync(BuildNamespace + pi.Build);
			if (string.IsNullOrEmpty(bstr))
			{
				HttpContext.Response.StatusCode = 404;
				return "build not found";
			}

			var b = JsonConvert.DeserializeObject<Build>(bstr);

			if (b.Creator != uid)
			{
				HttpContext.Response.StatusCode = 403;
				return "cannot modify other user's build";
			}

			if (b.Images != null)
			{
				var newList = b.Images.Where(i => i.Id != pi.Image);
				if (newList.Count() == b.Images.Count)
				{
					HttpContext.Response.StatusCode = 404;
					return "image not found";
				}

				b.Images = newList.ToList();
				await this._data.UpdateKeyAsync(BuildNamespace + pi.Build, JsonConvert.SerializeObject(b));
			}
			else
			{
				HttpContext.Response.StatusCode = 400;
				return "no images to delete";
			}

			return "Success";
		}


		[HttpGet("recent")]
		[Produces("application/json", "text/json")]
		public async Task<List<string>> GetRecent()
		{
			//FIXME
			var r = await this._data.GetKeyAsync(RecentBuildNamespace + RecentBuildName);
			if (string.IsNullOrEmpty(r))
			{
				return new List<string>();
			}

			return JsonConvert.DeserializeObject<List<string>>(r);
		}



		[HttpPost("save")]
		public async Task<string> Save([FromBody]Build b)
		{
			if (b != null && !string.IsNullOrEmpty(b.Creator))
			{
				var uid = _ua.AuthenticateUser(this);
				if (uid == null)
				{
					HttpContext.Response.StatusCode = 401;
					return "login first";
				}

				if (uid.UserId != b.Creator)
				{
					HttpContext.Response.StatusCode = 403;
					return "not your build";
				}

				var actual = await this.GetById(b.Id);

				// do not allow update of creator or images
				b.Creator = actual.Creator;
				b.Images = actual.Images;

				await this._data.UpdateKeyAsync(BuildNamespace + b.Id, JsonConvert.SerializeObject(b));
				await ListHelper.EnqueueList(_data, BuildController.RecentBuildNamespace, BuildController.RecentBuildName, b.Id);

				return "saved";
			}
			else
			{
				HttpContext.Response.StatusCode = 400;
				return "Invalid Build";
			}
		}

	}
}
