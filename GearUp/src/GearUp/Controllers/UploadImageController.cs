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

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace GearUp.Controllers.Controllers
{
	[Route("api/[controller]")]
	public class UploadImageController : Controller
	{

		public class UploadImageResult
		{
			public string Message { get; set; }
			public string Guid {get; set;}

		}


		private readonly BlobService _blobService;
		private readonly ILogger _logger;
		private readonly DocumentDB _ddb;
		private readonly string _imagesContainer;


		public UploadImageController(SiteSettings settings, ILogger logger, DocumentDB ddb, BlobService bs)
		{
			this._blobService = bs;
			this._logger = logger;
			this._ddb = ddb;
			this._imagesContainer = settings.ImagesContainer;

		}

		public readonly string[] ValidContentTypes = { "image/png", "image/jpeg", "image/gif"};

		// POST api/values
		[HttpPost]
		[Produces("application/json", "text/json")]
		public async Task<UploadImageResult> Post([FromQuery]string buildid)
		{
			var stream = Request.Body;
			var result = new UploadImageResult()
			{
				Message = "Error",
				Guid = Guid.NewGuid().ToString()
			};

			if (!ValidContentTypes.Contains(Request.ContentType))
			{
				result.Message = "Invalid Content Type";
				await Task.Yield(); //fixme what do i do here?
				return result;
			}

			if (string.IsNullOrEmpty(buildid))
			{
				result.Message = "Invalid Build ID";
				await Task.Yield();	//fixme what do i do here?
				return result;
			}

			_logger.WriteInformation("Upload Image, Content Type: " + Request.ContentType + " Build ID: " + buildid);

			// upload stream
			await this._blobService.UploadFile(stream, Request.ContentType, this._imagesContainer, result.Guid);
			result.Message = "Uploaded";

			// add pointer to document db
			var uid = UserLogin.UserUniqueId(User.Identity);
			await this._ddb.AddImageToBuildAsync(buildid, result.Guid, uid);

			return result;

		}

		// DELETE api/values/5
		[HttpDelete("{id}")]
		public void Delete(int id)
		{
		}
	}
}
