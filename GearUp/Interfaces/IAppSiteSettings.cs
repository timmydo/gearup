
namespace GearUp.Interfaces
{
    public interface IAppSiteSettings
    {
        string BlobStorageConnectionString { get; set; }
        string BlobLogContainer { get; set; }
        string BlobEndpoint { get; set; }
        string CdnEndpoint { get; set; }
        string RedisEndpoint { get; set; }
        string ImagesContainer { get; set; }

        string ServiceJSFileRoot { get; set; }

        string DocumentDatabaseId { get; set; }
        string DocumentCollectionId { get; set; }
        string DocumentEndpoint { get; set; }
        string DocumentKey { get; set; }
        string SearchName { get; set; }
        string SearchIndexName { get; set; }
        string SearchApiKey { get; set; }
        string SearchQueryKey { get; set; }
        string AppInsightsKey { get; set; }
    }

}