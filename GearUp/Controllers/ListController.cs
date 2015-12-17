
namespace GearUp.Controllers
{
	using GearUp.Auth;
	using GearUp.Interfaces;
	using GearUp.Models;
	using Microsoft.AspNet.Mvc;
	using Microsoft.Extensions.Logging;
	using Newtonsoft.Json.Linq;
	using Shared.Interfaces;
	using System;
	using System.Threading.Tasks;

	[Route("api/[controller]")]
    public class ListController : Controller
    {

        private readonly ILogger _logger;
        private readonly IPartitionedKeyValueDictionary _data;

        public ListController(IPartitionedKeyValueDictionary ddb, ILogger logger)
        {
            this._data = ddb;
            this._logger = logger;
        }


        [Produces("application/json", "text/json")]
        [HttpGet("get/{id}")]
        public async Task<string> GetById(string id)
        {
            return await this._data.GetKeyAsync("lists/" + id);
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
			//fixme
            //var b = await this._data.GetBuildAsync(pi.Build);
            //var l = await this._data.GetListAsync(pi.List);

            //await this._data.AddBuildToListAsync(b.id, l.id, uid);

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
			//fixme
			//var b = await this._data.GetBuildAsync(pi.Build);
			//var l = await this._data.GetListAsync(pi.List);

			//await this._data.RemoveBuildFromListAsync(b.id, l.id, uid);

			return "Success";
		}
        
        
        [HttpPost("delete")]
		public async Task<string> Delete([FromBody]BuildList b)
		{
			if (b != null)
			{
				var uid = UserLogin.UserUniqueId(User?.Identity);
				//fix me ensure creator is the current user
				await this._data.DeleteKeyAsync("lists/" + b);
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
				var newList = JArray.FromObject(b);
				//fixme check creator
				await this._data.UpdateKeyAsync("lists/" + b, newList.ToString(), "timestamp");
                return "success";
			}
			else
			{
				HttpContext.Response.StatusCode = 400;
				return "bad";
			}
		}

    }
}
