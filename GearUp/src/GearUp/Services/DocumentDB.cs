using GearUp.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Framework.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GearUp.Services
{
    public class DocumentDB
    {

		private SiteSettings _settings;
		private ILogger _logger;
		public DocumentDB(SiteSettings settings, ILogger logger)
		{
			this._settings = settings;
			this._logger = logger;
			logger.WriteInformation("DocumentDB creation");
			this.ReadOrCreateDatabase();
			this.ReadOrCreateCollection(Database.SelfLink);
		}

		private Database database;
		private Database Database
		{
			get
			{
				if (database == null)
				{
					database = ReadOrCreateDatabase();
				}

				return database;
			}
		}

		private DocumentCollection collection;
		private DocumentCollection Collection
		{
			get
			{
				if (collection == null)
				{
					collection = ReadOrCreateCollection(Database.SelfLink);
				}

				return collection;
			}
		}

		private DocumentClient client;
		private DocumentClient Client
		{
			get
			{
				if (client == null)
				{
					string endpoint = this._settings.DocumentEndpoint;
					string authKey = this._settings.DocumentKey;
					Uri endpointUri = new Uri(endpoint);
					client = new DocumentClient(endpointUri, authKey);
				}

				return client;
			}
		}

		private DocumentCollection ReadOrCreateCollection(string databaseLink)
		{
			var col = Client.CreateDocumentCollectionQuery(databaseLink)
							  .Where(c => c.Id == this._settings.DocumentCollectionId)
							  .AsEnumerable()
							  .FirstOrDefault();

			if (col == null)
			{
				col = Client.CreateDocumentCollectionAsync(databaseLink, new DocumentCollection { Id = this._settings.DocumentCollectionId }).Result;
			}

			return col;
		}

		private Database ReadOrCreateDatabase()
		{
			var db = Client.CreateDatabaseQuery()
							.Where(d => d.Id == this._settings.DocumentDatabaseId)
							.AsEnumerable()
							.FirstOrDefault();

			if (db == null)
			{
				db = Client.CreateDatabaseAsync(new Database { Id = this._settings.DocumentDatabaseId }).Result;
			}

			return db;
		}


		public async Task<Document> CreateBuildAsync(Build item)
		{
			this._logger.WriteInformation("Creating build, id: " + item.Id);
			return await Client.CreateDocumentAsync(Collection.SelfLink, item);
		}

		public Build GetBuild(string id)
		{
			var b = Client.CreateDocumentQuery<Build>(Collection.DocumentsLink)
						.Where(d => d.Id == id)
						.AsEnumerable()
						.FirstOrDefault();
			//this._logger.WriteInformation("GetBuild id = " + id + " returns " + b == null ? "null" :  b.ToString());
			this._logger.WriteInformation("GetBuild id = " + id);

			return b;
		}

		public async Task<Document> UpdateBuildAsync(Build item)
		{
			this._logger.WriteInformation("Updating build, id: " + item.Id);
			Document doc = Client.CreateDocumentQuery(Collection.DocumentsLink)
								.Where(d => d.Id == item.Id)
								.AsEnumerable()
								.FirstOrDefault();

			return await Client.ReplaceDocumentAsync(doc.SelfLink, item);
		}

	}
}