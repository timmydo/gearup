
namespace GearUp.Controllers
{
	using GearUp.Auth;
	using GearUp.Interfaces;
	using GearUp.Models;
	using Microsoft.AspNet.Mvc;
	using Microsoft.Extensions.Logging;
	using System;
	using System.Threading.Tasks;

	[Route("api/[controller]")]
    public class ListController : Controller
    {

        private readonly ILogger _logger;
        private readonly IAppDataService _data;

        public ListController(IAppDataService ddb, ILogger logger)
        {
            this._data = ddb;
            this._logger = logger;
        }


        [Produces("application/json", "text/json")]
        [HttpGet("get/{id}")]
        public async Task<BuildList> GetById(string id)
        {
            var b = await this._data.GetListAsync(id);
            if (b == null)
            {
                b = new BuildList();
                b.id = id;
                var uid = UserLogin.UserUniqueId(User?.Identity);
                b.Creator = uid;
                if (string.IsNullOrEmpty(uid))
                {
                    b.Title = "Must log in to create lists";
                    return b;
                }
                this._logger.LogInformation("Setting creator to " + uid);
                await this._data.CreateListAsync(b);
            }
            return b;
        }


        public class ParamInfo
        {
            public string Build { get; set; }
            public string List { get; set; }

        }


        [HttpPost("add")]
        public async Task<string> AddBuild([FromBody]ParamInfo pi)
        {
            var uid = UserLogin.UserUniqueId(User?.Identity);

            if (string.IsNullOrEmpty(uid))
            {
                throw new Exception("User is not logged in");
            }

            var b = await this._data.GetBuildAsync(pi.Build);
            var l = await this._data.GetListAsync(pi.List);

            await this._data.AddBuildToListAsync(b.id, l.id, uid);

            return "Success";
        }
        
        

		[HttpPost("remove")]
		public async Task<string> RemoveBuild([FromBody]ParamInfo pi)
		{
			var uid = UserLogin.UserUniqueId(User?.Identity);

			if (string.IsNullOrEmpty(uid))
			{
				throw new Exception("User is not logged in");
			}

			var b = await this._data.GetBuildAsync(pi.Build);
			var l = await this._data.GetListAsync(pi.List);

			await this._data.RemoveBuildFromListAsync(b.id, l.id, uid);

			return "Success";
		}
        
        
        [HttpPost("delete")]
		public async Task<string> Delete([FromBody]BuildList b)
		{
			if (b != null)
			{
				var uid = UserLogin.UserUniqueId(User?.Identity);
				await this._data.DeleteListAsync(b, uid);
				return "Deleted";
			}
			else
			{
				throw new Exception("Invalid List");
			}
		}
        
        [HttpPost("save")]
		public async Task<string> Save([FromBody]BuildList b)
		{
			if (b != null && !string.IsNullOrEmpty(b.Creator))
			{
				var uid = UserLogin.UserUniqueId(User?.Identity);
				var newId = await this._data.SaveListAsync(b, uid);
                return newId;
			}
			else
			{
				throw new Exception("Invalid List");
			}
		}

    }
}
