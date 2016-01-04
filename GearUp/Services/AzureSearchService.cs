namespace GearUp.Services
{
	using GearUp.Interfaces;
	using Microsoft.Extensions.Options;
	using Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Microsoft.Azure.Search;
	using Microsoft.Azure.Search.Models;
	using Microsoft.Extensions.Logging;
	using Newtonsoft.Json;
	public class AzureSearchService : IAppSearchService
	{
		SearchServiceClient _serviceClient;
		Index _index;
		SearchIndexClient _indexClient;

		private const string _buildIndexName = "builds";
		private ILogger _log;

		public class SearchFriendlyBuild
		{
			public string Id { get; set; }
			public string Title { get; set; }
			public List<string> Parts { get; set; }
			public string Json { get; set; }

			public static SearchFriendlyBuild FromBuild(Build b)
			{
				var sfb = new SearchFriendlyBuild()
				{
					Id = b.Id,
					Title = b.Title,
					Json = JsonConvert.SerializeObject(b),
					Parts = b.Parts.Select(p => p.Title).ToList()
				};

				return sfb;
			}

		}

		public AzureSearchService(IOptions<SiteSettings> ss, ILogger log)
		{
			if (ss.Value.SearchApiKey == null)
			{
				throw new ArgumentNullException("AppSettings:SearchApiKey is not set");
			}

			this._serviceClient = new SearchServiceClient(ss.Value.SearchName, new SearchCredentials(ss.Value.SearchApiKey));
			this._log = log;
			this._index = new Index()
			{
				Name = _buildIndexName,
				Fields = new[]
				{
					new Field("Id", DataType.String)                       { IsKey = true },
					new Field("Title", DataType.String)                    { IsSearchable = true},
					new Field("Parts", DataType.Collection(DataType.String))     { IsSearchable = true},
					new Field("Json", DataType.String)     { IsSearchable = false}
				},
				Suggesters = new List<Suggester>() {
					new Suggester("default", SuggesterSearchMode.AnalyzingInfixMatching, "Title", "Parts")
				},
				CorsOptions = new CorsOptions(new List<string> { "*" })
			};

			this._serviceClient.Indexes.CreateOrUpdate(this._index);
			this._indexClient = this._serviceClient.Indexes.GetClient(_buildIndexName);
		}

		public async Task<Build[]> SearchBuildsAsync(string query)
		{
			var response = await this._indexClient.Documents.SearchAsync<SearchFriendlyBuild>(query);
			var results = response.Results;
			var builds = results.Select(r => JsonConvert.DeserializeObject<Build>(r.Document.Json));
			return builds.ToArray();
		}

		public async Task AddBuildsToIndexAsync(IEnumerable<Build> b)
		{
			var batch = IndexBatch.Create(b.Select(doc => IndexAction.Create(IndexActionType.MergeOrUpload, SearchFriendlyBuild.FromBuild(doc))));
			try
			{
				await this._indexClient.Documents.IndexAsync(batch);
			}
			catch (IndexBatchException e)
			{
				_log.LogError("Failed to index some of the documents: {0}, due to: {1}",
					String.Join(", ", e.IndexResponse.Results.Where(r => !r.Succeeded).Select(r => r.Key)),
					e.Message);
			}
		}

		public async Task RemoveBuildsFromIndexAsync(IEnumerable<Build> b)
		{
			var batch = IndexBatch.Create(b.Select(doc => IndexAction.Create(IndexActionType.Delete, SearchFriendlyBuild.FromBuild(doc))));
			try
			{
				await this._indexClient.Documents.IndexAsync(batch);
			}
			catch (IndexBatchException e)
			{
				_log.LogError("Failed to de-index some of the documents: {0}, due to: {1}",
					String.Join(", ", e.IndexResponse.Results.Where(r => !r.Succeeded).Select(r => r.Key)),
					e.Message);
			}
		}

		
	}
}
