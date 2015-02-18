using GearUp.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Framework.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GearUp.Services
{
	public class DocumentDB
	{

		private SiteSettings _settings;
		private ILogger _logger;
		private StoredProcedure _addImageToBuild;
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

		public async Task EnsureStoredProcs()
		{
			if (this._addImageToBuild == null)
			{
				await LoadStoredProcs();
			}
		}

		private async Task LoadStoredProcs()
		{
			string addImageToBuildJs = @"Services\js\addImageToBuild.js";

			var sproc = new StoredProcedure
			{
				Id = Path.GetFileNameWithoutExtension(addImageToBuildJs),
				Body = File.ReadAllText(addImageToBuildJs)
			};

			await TryDeleteStoredProcedure(this.Collection.SelfLink, sproc.Id);

			this._logger.WriteInformation("Creating stored procedure " + sproc.Id);
			sproc = await client.CreateStoredProcedureAsync(this.Collection.SelfLink, sproc);
			this._addImageToBuild = sproc;

		}

		private async Task TryDeleteStoredProcedure(string colSelfLink, string sprocId)
		{
			StoredProcedure sproc = Client.CreateStoredProcedureQuery(colSelfLink).Where(s => s.Id == sprocId).AsEnumerable().FirstOrDefault();
			if (sproc != null)
			{
				this._logger.WriteInformation("Deleting stored procedure " + sprocId);
				await client.DeleteStoredProcedureAsync(sproc.SelfLink);
			}
		}

		public async Task AddImageToBuildAsync(string buildGuid, string imageGuid)
		{
			await EnsureStoredProcs();
			this._logger.WriteInformation("Executing stored procedure addImageToBuild(" + buildGuid + ", " + imageGuid + ")");
			var response = await this.Client.ExecuteStoredProcedureAsync<string>(this._addImageToBuild.SelfLink, buildGuid, imageGuid);
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

	}
}