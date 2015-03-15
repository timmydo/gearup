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
	public class DeleteBuildController : Controller
	{


		private readonly ILogger _logger;
		private readonly DocumentDB _ddb;

		public DeleteBuildController(SiteSettings settings, ILogger logger, DocumentDB ddb)
		{
			this._logger = logger;
			this._ddb = ddb;
		}


		// POST api/values
		[HttpPost]
		public async Task<string> Post([FromBody]Build b)
		{
			if (b != null)
			{
				var uid = UserLogin.UserUniqueId(User.Identity);
				await this._ddb.DeleteBuildAsync(b, uid);
				return "Deleted";
			}
			else
			{
				throw new Exception("Invalid Build");
			}
		}

		// DELETE api/values/5
		[HttpDelete("{id}")]
		public void Delete(int id)
		{
		}
	}
}
