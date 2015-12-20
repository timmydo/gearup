namespace GearUp.Controllers
{
	using GearUp.Auth;
	using GearUp.Interfaces;
	using GearUp.Models;
	using Microsoft.AspNet.Mvc;
	using Microsoft.Extensions.Logging;
	using Microsoft.ServiceFabric.Services.Remoting.Client;
	using Newtonsoft.Json.Linq;
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

		public readonly string BuildNamespace = "b/";

        public class DeleteImageParamInfo
        {
            public string Build { get; set; }
            public string Image { get; set; }

        }


        public BuildController(ILogger logger, IPartitionedKeyValueDictionary data, IAppBlobStorage blobService)
        {
            this._logger = logger;
			this._data = data;
			this._blobService = blobService;
        }


        [Produces("application/json", "text/json")]
        [HttpGet("get/{id}")]
        public async Task<string> GetById(string id)
        {
            var b = await this._data.GetKeyAsync(BuildNamespace + id);
			if (string.IsNullOrEmpty(b))
			{
				HttpContext.Response.StatusCode = 404;
			}

            return b;
        }


		[HttpPost("create")]
		public async Task<string> CreateBuild()
		{
			var guid = System.Guid.NewGuid().ToString("N");
            var uid = UserLogin.UserUniqueId(User?.Identity);
			var ownerstr = uid == null ? "null" : "\"" + uid + "\"";

			await this._data.AddKeyAsync(BuildNamespace + guid, 
				string.Format("{{'id': '{0}', '{1}': {2}}}", guid, Build.CreatorFieldName, ownerstr));
			return guid;
		}

		[HttpPost("delete")]
        public async Task<string> DeleteBuild([FromBody]string b)
        {
            if (!string.IsNullOrEmpty(b))
            {
                var uid = UserLogin.UserUniqueId(User?.Identity);
				var bdata = await this._data.GetKeyAsync(BuildNamespace + b);
				if (string.IsNullOrEmpty(bdata))
				{
					HttpContext.Response.StatusCode = 404;
					return "not found";
				}
				else
				{
					var jo = JObject.Parse(bdata);
					var actualBuild = JObject.Parse(await this._data.GetKeyAsync(BuildNamespace + b));
					if (uid == actualBuild.GetValue(Build.CreatorFieldName).ToObject<string>())
					{
						await this._data.DeleteKeyAsync(BuildNamespace + b);
					}
					else
					{
						HttpContext.Response.StatusCode = 403;
						return "you are not the owner";
					}

					return "Deleted";
				}
            }
            else
            {
                throw new Exception("Invalid Build");
            }
        }


        [Produces("application/json", "text/json")]
        [HttpPost("get")]
		public async Task<string> GetMultiple([FromBody]BuildList bl)
        {
            this._logger.LogInformation("Get builds, count: " + bl.Builds.Count);
            if (bl.Builds.Count < 1)
            {
                return "[]";
            }
            var list = new JArray();
            foreach (var id in bl.Builds)
            {
                var fullBuild = await this._data.GetKeyAsync(BuildNamespace + id);
                if (!string.IsNullOrEmpty(fullBuild))
                {
					var jobj = JObject.Parse(fullBuild);
					list.Add(jobj);
                }
            }

            return list.ToString();
        }


		public readonly List<string> ValidContentTypes = new List<string>() { "image/png", "image/jpeg", "image/gif" };

		[HttpPost("add-image")]
		[Produces("application/json", "text/json")]
		public async Task<string> AddImage([FromQuery]string buildid)
		{
			var stream = Request.Body;
			var uid = UserLogin.UserUniqueId(User?.Identity);

			if (string.IsNullOrEmpty(uid))
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

			var b = await this._data.GetKeyAsync(BuildNamespace + buildid);
			if (string.IsNullOrEmpty(b))
			{
				HttpContext.Response.StatusCode = 400;
				return "build not found";
			}

			_logger.LogInformation("Upload Image, Content Type: " + Request.ContentType + " Build ID: " + buildid);

			// upload stream
			var imageGuid = await this._blobService.UploadUserImage(stream, Request.ContentType);

			var bobj = JObject.Parse(b);
			var arr = bobj[Build.ImageFieldName] as JArray;
			if (arr == null)
			{
				arr = JArray.Parse("[]");
			}

			arr.Add(JToken.Parse(string.Format("{{'{0}':'{1}'}}", Image.GuidFieldName, imageGuid)));
			bobj[Build.ImageFieldName] = arr;

			var success = await this._data.UpdateKeyAsync(BuildNamespace + buildid, bobj.ToString(), "timestamp");

			if (!success)
			{
				//FIXME, try again?
				HttpContext.Response.StatusCode = 500;
				return "cannot update build";
			}

			return imageGuid;
		}

		[HttpPost("delete-image")]
        public async Task<string> DeleteImage([FromBody]DeleteImageParamInfo pi)
        {
            var uid = UserLogin.UserUniqueId(User?.Identity);

            if (string.IsNullOrEmpty(uid))
            {
				HttpContext.Response.StatusCode = 401;
                return "User is not logged in";
            }

			var b = await this._data.GetKeyAsync(BuildNamespace + pi.Build);
			if (string.IsNullOrEmpty(b))
			{
				HttpContext.Response.StatusCode = 400;
				return "build not found";
			}

			var bobj = JObject.Parse(b);

			var buid = bobj.GetValue(Build.CreatorFieldName).ToObject<string>();
			if (buid != uid)
			{
				HttpContext.Response.StatusCode = 403;
				return "cannot modify other user's build";
			}

			var imgList = bobj[Build.ImageFieldName] as JArray;
			if (imgList != null)
			{
				var newList = imgList.Where(i => i[Image.GuidFieldName].ToObject<string>() != pi.Image);
				bobj[Build.ImageFieldName] = JArray.FromObject(newList);
				await this._data.UpdateKeyAsync(BuildNamespace + pi.Build, bobj.ToString(), "timestamp");
			}

            return "Success";
        }


        [HttpGet("recent")]
        [Produces("application/json", "text/json")]
        public async Task<string> GetRecent()
        {

			return await Task.FromResult("[]");
        }



        [HttpPost("save")]
        public async Task<string> Save([FromBody]Build b)
        {
            if (User == null)
            {
                throw new Exception("User is null");
            }

            if (b != null && !string.IsNullOrEmpty(b.Creator))
            {
                var uid = UserLogin.UserUniqueId(User?.Identity);
				if (uid != b.Creator)
				{
					HttpContext.Response.StatusCode = 403;
					return "not your build";
				}

                this._logger.LogInformation("SaveBuild Controller Post");
				var bobj = JObject.FromObject(b);
                await this._data.UpdateKeyAsync(BuildNamespace + b.id, bobj.ToString(), "timestamp");
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
