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
		public readonly string BuildNamespace = "b/";

        public class DeleteImageParamInfo
        {
            public string Build { get; set; }
            public string Image { get; set; }

        }


        public BuildController(ILogger logger, IPartitionedKeyValueDictionary data)
        {
            this._logger = logger;
			this._data = data;
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
				string.Format("{{'id': '{0}', owner: {1}}}", guid, ownerstr));
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
					if (uid == actualBuild.GetValue("owner").ToObject<string>())
					{
						await this._data.DeleteKeyAsync(BuildNamespace + b);
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
                if (fullBuild != null)
                {
                    list.Add(fullBuild);
                }
            }

            return list.ToString();
        }


        [HttpPost("delete-image")]
        public async Task<string> DeleteImage([FromBody]DeleteImageParamInfo pi)
        {
            var uid = UserLogin.UserUniqueId(User?.Identity);

            if (string.IsNullOrEmpty(uid))
            {
                throw new Exception("User is not logged in");
            }

			var b = await this._data.GetKeyAsync(BuildNamespace + pi.Build);
			if (string.IsNullOrEmpty(b))
			{
				HttpContext.Response.StatusCode = 400;
				return "build not found";
			}

			var bobj = JObject.Parse(b);

			var buid = bobj.GetValue("Creator").ToObject<string>();
			if (buid != uid)
			{
				HttpContext.Response.StatusCode = 403;
				return "cannot modify other user's build";
			}

			var imgList = bobj["img"] as JArray;
			if (imgList != null)
			{
				var newList = imgList.Where(i => i["Guid"].ToObject<string>() != pi.Image);
				bobj["img"] = JArray.FromObject(newList);
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
