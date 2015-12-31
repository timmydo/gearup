namespace GearUp.Services
{
	using Microsoft.Extensions.Logging;
	using Microsoft.WindowsAzure.Storage;
	using System.IO;
	using System.Threading.Tasks;
	using GearUp.Interfaces;
	using Microsoft.Extensions.OptionsModel;
	using Models;
	using System;
	using System.Security.Cryptography;
	using Microsoft.AspNet.WebUtilities;
	public class AzureBlobService : IAppBlobStorage
    {
		private SiteSettings _settings;
		private ILogger _logger;
		private readonly CloudStorageAccount _storageAccount;

		public AzureBlobService(IOptions<SiteSettings> settings, ILogger logger)
		{
			this._settings = settings.Value;
			this._logger = logger;
			this._storageAccount = CloudStorageAccount.Parse(this._settings.StorageConnectionString);

			logger.LogInformation("BlobService creation");
		}

		private async Task UploadFile(Stream stream, string contentType, string containerName, string uid)
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
		public async Task<bool> DeleteImage(string uid)
		{
			return await this.DeleteFile(this._settings.ImagesContainer, uid);
		}

		public async Task<string> UploadUserImage(Stream stream, string contentType)
		{
			var hash = SHA256.Create();
			var ms = new MemoryStream();
			stream.CopyTo(ms);
			ms.Seek(0, SeekOrigin.Begin);
			var guid = WebEncoders.Base64UrlEncode(hash.ComputeHash(ms));
			ms.Seek(0, SeekOrigin.Begin);
			await this.UploadFile(ms, contentType, this._settings.ImagesContainer, guid);
			return guid;
		}
	}
}