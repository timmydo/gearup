using System;

namespace GearUp.Services
{
    public class SiteSettings
    {
		public string BlobStorageConnectionString { get; set; }
		public string BlobEndpoint { get; set; }
		public string CdnEndpoint { get; set; }
		public string RedisEndpoint { get; set; }
		public string ImagesContainer { get; set; }

		public string ServiceJSFileRoot { get; set; }

		public string DocumentDatabaseId { get; set; }
		public string DocumentCollectionId { get; set; }
		public string DocumentEndpoint { get; set; }
		public string DocumentKey { get; set; }
	}
}