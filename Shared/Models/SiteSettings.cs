using System;

namespace GearUp.Models
{
    public class SiteSettings
	{
		public string AppInsightsKey { get; set; }
		public string StorageConnectionString { get; set; }
		public string TableStorageTableName { get; set; }
		public string BlobEndpoint { get; set; }
		public string CdnEndpoint { get; set; }
		public string ImagesContainer { get; set; }
		public string SearchName { get; set; }
		public string SearchApiKey { get; set; }
		public string CookieHmacKey { get; set; }

		
	}
}