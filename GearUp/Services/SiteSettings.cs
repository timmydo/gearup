using System;

namespace GearUp.Services
{
    public class SiteSettings
    {
		public string BlobStorageConnectionString { get; set; }
		public string BlobEndpoint { get; set; }
		public string CdnEndpoint { get; set; }
		public string ImagesContainer { get; set; }
		public string GoogleClientId { get; set; }
		public string GoogleClientSecret { get; set; }
		public string FacebookClientId { get; set; }
		public string FacebookClientSecret { get; set; }
	}
}