﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace GearUp.Controllers.Controllers
{
	[Route("api/[controller]")]
	public class UploadImageController : Controller
	{

		public class UploadImageResult
		{
			public string Message { get; set; }
			public string[] Guids {get; set;}

		}


		private CloudStorageAccount _storageAccount;


		public UploadImageController(SiteSettings settings)
		{
			this._storageAccount = CloudStorageAccount.Parse(settings.BlobStorageConnectionString);
		}


		// POST api/values
		[HttpPost]
		[Produces("application/json", "text/json")]
		public async Task<UploadImageResult> Post(IList<IFormFile> files)
		{
			var result = new UploadImageResult();
			var list = new List<string>();
			var client = this._storageAccount.CreateCloudBlobClient();
			var container = client.GetContainerReference("uploadimages");
			
			foreach (var f in files)
			{
				if (f.ContentType == "blah") {

				}
				using (var stream = f.OpenReadStream())
				{
					var guid = Guid.NewGuid().ToString();
					var blob = container.GetBlockBlobReference(guid);
					await blob.UploadFromStreamAsync(stream);
					list.Add(guid);
				}
			}
			result.Message = "fixme";
			result.Guids = list.ToArray();

			return result;

		}

		// DELETE api/values/5
		[HttpDelete("{id}")]
		public void Delete(int id)
		{
		}
	}
}
