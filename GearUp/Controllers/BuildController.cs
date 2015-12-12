namespace GearUp.Controllers
{
	using GearUp.Auth;
	using GearUp.Interfaces;
	using GearUp.Models;
	using Microsoft.AspNet.Mvc;
	using Microsoft.Extensions.Logging;
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;


	[Route("api/[controller]")]
    public class BuildController : Controller
    {

        private readonly ILogger _logger;
        private readonly IAppDataService _data;

        public class DeleteImageParamInfo
        {
            public string Build { get; set; }
            public string Image { get; set; }

        }


        public BuildController(IAppDataService ddb, ILogger logger)
        {
            this._data = ddb;
            this._logger = logger;
        }


        [Produces("application/json", "text/json")]
        [HttpGet("get/{id}")]
        public async Task<Build> GetById(string id)
        {
            var b = await this._data.GetBuildAsync(id);
            if (b == null)
            {
                b = new Build();
                b.id = id;
                var uid = UserLogin.UserUniqueId(User?.Identity);
                
                b.Creator = uid;
                if (string.IsNullOrEmpty(uid))
                {
                    b.Title = "Must log in to create builds";
                    return b;
                }
                this._logger.LogInformation("Setting creator to " + uid);
                await this._data.CreateBuildAsync(b);
            }
            return b;
        }

        [HttpPost("delete")]
        public async Task<string> DeleteBuild([FromBody]Build b)
        {
            if (b != null)
            {
                var uid = UserLogin.UserUniqueId(User?.Identity);
                await this._data.DeleteBuildAsync(b, uid);
                return "Deleted";
            }
            else
            {
                throw new Exception("Invalid Build");
            }
        }


        [HttpPost("get")]
        public async Task<Build[]> GetMultiple([FromBody]BuildList bl)
        {
            this._logger.LogInformation("Get builds, count: " + bl.Builds.Count);
            if (bl.Builds.Count < 1)
            {
                return new Build[0];
            }
            var list = new List<Build>();
            foreach (var id in bl.Builds)
            {
                var fullBuild = await this._data.GetBuildAsync(id);
                if (fullBuild != null)
                {
                    list.Add(fullBuild);
                }
            }

            return list.ToArray();
        }


        [HttpPost("delete-image")]
        public async Task<string> DeleteImage([FromBody]DeleteImageParamInfo pi)
        {
            var uid = UserLogin.UserUniqueId(User?.Identity);

            if (string.IsNullOrEmpty(uid))
            {
                throw new Exception("User is not logged in");
            }

            await this._data.DeleteImageFromBuildAsync(pi.Build, pi.Image, uid);

            return "Success";
        }


        [HttpGet("recent")]
        [Produces("application/json", "text/json")]
        public async Task<string[]> Get()
        {
            return await this._data.GetRecentBuildsAsync();
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
                
                this._logger.LogInformation("SaveBuild Controller Post");
                var newId = await this._data.SaveBuildAsync(b, uid);
                return newId;
            }
            else
            {
                throw new Exception("Invalid Build");
            }
        }

    }
}
