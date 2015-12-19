namespace GearUp.Services
{
	using Microsoft.Extensions.Logging;
	using Microsoft.WindowsAzure.Storage;
	using System.IO;
	using System.Threading.Tasks;
	using GearUp.Interfaces;
	using Microsoft.Extensions.OptionsModel;
	using Models;
	public class AzureBlobService : IAppBlobStorage
    {
		private SiteSettings _settings;
		private ILogger _logger;
		private readonly CloudStorageAccount _storageAccount;

		public AzureBlobService(IOptions<SiteSettings> settings, ILogger logger)
		{
			this._settings = settings.Value;
			this._logger = logger;
			this._storageAccount = CloudStorageAccount.Parse(this._settings.BlobStorageConnectionString);

			logger.LogInformation("BlobService creation");
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