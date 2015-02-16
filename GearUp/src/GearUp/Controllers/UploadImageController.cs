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


		private readonly CloudStorageAccount _storageAccount;
		private readonly ILogger _logger;
		private readonly DocumentDB _ddb;

		public UploadImageController(SiteSettings settings, ILogger logger, DocumentDB ddb)
		{
			this._storageAccount = CloudStorageAccount.Parse(settings.BlobStorageConnectionString);
			this._logger = logger;
			this._ddb = ddb;
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

			var client = this._storageAccount.CreateCloudBlobClient();
			var container = client.GetContainerReference("uploadimages");

			var blob = container.GetBlockBlobReference(result.Guid);
			await blob.UploadFromStreamAsync(stream);
			blob.Properties.ContentType = Request.ContentType;
			await blob.SetPropertiesAsync();
			result.Message = "Uploaded";
			await this._ddb.AddImageToBuildAsync(buildid, result.Guid);

			return result;

		}

		// DELETE api/values/5
		[HttpDelete("{id}")]
		public void Delete(int id)
		{
		}
	}
}
