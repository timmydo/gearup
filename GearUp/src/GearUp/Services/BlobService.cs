using Microsoft.Framework.Logging;
using Microsoft.WindowsAzure.Storage;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GearUp.Services
{
    public class BlobService
    {
		private SiteSettings _settings;
		private ILogger _logger;
		private readonly CloudStorageAccount _storageAccount;


		public BlobService(SiteSettings settings, ILogger logger)
		{
			this._settings = settings;
			this._logger = logger;
			this._storageAccount = CloudStorageAccount.Parse(settings.BlobStorageConnectionString);

			logger.WriteInformation("BlobService creation");
		}


		public async Task UploadFile(Stream stream, string contentType, string containerName, string uid)
		{
			var client = this._storageAccount.CreateCloudBlobClient();
			var container = client.GetContainerReference(containerName);
			var blob = container.GetBlockBlobReference(uid);
			await blob.UploadFromStreamAsync(stream);
			blob.Properties.ContentType = contentType;
			await blob.SetPropertiesAsync();

		}

		public async Task<bool> DeleteFile(string containerName, string uid)
		{
			var client = this._storageAccount.CreateCloudBlobClient();
			var container = client.GetContainerReference(containerName);
			var blob = container.GetBlockBlobReference(uid);
			return await blob.DeleteIfExistsAsync();
		}

	}
}