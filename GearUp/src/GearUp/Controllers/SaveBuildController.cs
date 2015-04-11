using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Framework.Logging;
using System.IO;
using GearUp.Services;
using GearUp.Models;
using Newtonsoft.Json;

namespace GearUp.Controllers.Controllers
{
	[Route("api/[controller]")]
	public class SaveBuildController : Controller
	{


		private readonly ILogger _logger;
		private readonly DataService _data;

		public SaveBuildController(SiteSettings settings, ILogger logger, DataService ddb)
		{
			this._logger = logger;
			this._data = ddb;
			
		}


		// POST api/values
		[HttpPost]
		public async Task<string> Post([FromBody]Build b)
		{
			if (User == null)
			{
				throw new Exception("User is null");
			}

			if (b != null && !string.IsNullOrEmpty(b.Creator))
			{
				var uid = UserLogin.UserUniqueId(User.Identity);
				this._logger.WriteInformation("SaveBuild Controller Post");
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
