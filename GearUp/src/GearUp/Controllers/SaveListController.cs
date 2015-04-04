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
	public class SaveListController : Controller
	{


		private readonly ILogger _logger;
		private readonly DocumentDB _ddb;

		public SaveListController(SiteSettings settings, ILogger logger, DocumentDB ddb)
		{
			this._logger = logger;
			this._ddb = ddb;
		}


		// POST api/values
		[HttpPost]
		public async Task<string> Post([FromBody]BuildList b)
		{
			if (b != null && !string.IsNullOrEmpty(b.Creator))
			{
				var uid = UserLogin.UserUniqueId(User.Identity);
				var newId = await this._ddb.SaveListAsync(b, uid);
                return newId;
			}
			else
			{
				throw new Exception("Invalid List");
			}
		}

	}
}
