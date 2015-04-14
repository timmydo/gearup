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
	public class RemoveBuildFromListController : Controller
	{


		private readonly ILogger _logger;
		private readonly DataService _data;

		public RemoveBuildFromListController(SiteSettings settings, ILogger logger, DataService ddb)
		{
			this._logger = logger;
			this._data = ddb;
		}

		public class ParamInfo
		{
			public string Build { get; set; }
			public string List { get; set; }

		}


		// POST api/values
		[HttpPost]
		public async Task<string> Post([FromBody]ParamInfo pi)
		{
			var uid = UserLogin.UserUniqueId(User.Identity);

			if (string.IsNullOrEmpty(uid))
			{
				throw new Exception("User is not logged in");
			}

			var b = await this._data.GetBuildAsync(pi.Build);
			var l = await this._data.GetListAsync(pi.List);

			await this._data.RemoveBuildFromListAsync(b.id, l.id, uid);

			return "Success";
		}

	}
}
