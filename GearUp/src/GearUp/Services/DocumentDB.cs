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

		public async Task AddImageToBuildAsync(string buildGuid, string imageGuid)
		{
			var b = GetBuild(buildGuid);
			if (b != null)
			{
				b.Images.Add(new Image() { Guid = imageGuid });
				await UpdateBuildAsync(b);
			}
			else
			{
				this._logger.WriteError("AddImageToBuild: Cannot find build: " + buildGuid);
			}
		}

		public async Task<Document> CreateBuildAsync(Build item)
		{
			this._logger.WriteInformation("Creating build, id: " + item.id);
			return await Client.CreateDocumentAsync(Collection.SelfLink, item);
		}

		public Build GetBuild(string id)
		{
			this._logger.WriteInformation("GetBuild id = " + id);
			var b = Client.CreateDocumentQuery<Build>(Collection.DocumentsLink)
						.Where(d => d.id == id)
						.AsEnumerable()
						.FirstOrDefault();

			return b;
		}

		public async Task UpdateBuildAsync(Build item)
		{
			this._logger.WriteInformation("Updating build, id: " + item.id);
			var doc = Client.CreateDocumentQuery(Collection.DocumentsLink)
								.Where(d => d.Id == item.id)
								.AsEnumerable()
								.FirstOrDefault();

			//var doc = Client.CreateDocumentQuery<Document>(Collection.DocumentsLink, new SqlQuerySpec
			//{
			//	QueryText = "SELECT * FROM Builds b WHERE b.id = @myid",
			//	Parameters = new SqlParameterCollection()
			//	{
			//		new SqlParameter("@myid",item.id)
			//	}
			//}).AsEnumerable().FirstOrDefault();




			if (doc == null)
			{
				this._logger.WriteError("Cannot find item id = " + item.id);
				return;
			}

			this._logger.WriteInformation("Replacing build selflink= " + doc.SelfLink);
			await Client.ReplaceDocumentAsync(doc.SelfLink, item);
		}

	}
}