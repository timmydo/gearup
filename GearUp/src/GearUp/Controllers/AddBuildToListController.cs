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
	public class AddBuildToListController : Controller
	{


		private readonly ILogger _logger;
		private readonly DocumentDB _ddb;

		public AddBuildToListController(SiteSettings settings, ILogger logger, DocumentDB ddb)
		{
			this._logger = logger;
			this._ddb = ddb;
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

			var b = this._ddb.GetBuild(pi.Build);
			var l = this._ddb.GetList(pi.List);

			await this._ddb.AddBuildToListAsync(b.id, l.id, uid);

			return "Success";
		}

	}
}
