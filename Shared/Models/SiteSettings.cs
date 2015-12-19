using System;

namespace GearUp.Models
{
    public class SiteSettings
	{
		public string AppInsightsKey { get; set; }
		public string BlobStorageConnectionString { get; set; }
		public string BlobEndpoint { get; set; }
		public string CdnEndpoint { get; set; }
		public string ImagesContainer { get; set; }
		public string SearchName { get; set; }
		public string SearchIndexName { get; set; }
		public string SearchQueryKey { get; set; }
		public string SearchApiKey { get; set; }
	}
}