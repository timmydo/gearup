﻿using GearUp.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Framework.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
		private StoredProcedure _addBuildToList;
		private StoredProcedure _removeBuildFromList;
		private StoredProcedure _removeImageFromBuild;
		private StoredProcedure _saveBuild;
		private StoredProcedure _saveList;
		private BlobService _blobService;
		private string _imagesContainer;
		private string _serviceJSRoot;

		public DocumentDB(SiteSettings settings, ILogger logger, BlobService bs)
		{
			this._settings = settings;
			this._logger = logger;
			this._blobService = bs;
			this._imagesContainer = settings.ImagesContainer;
			this._serviceJSRoot = settings.ServiceJSFileRoot;

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
				await LoadStoredProcs(false);
			}
		}

		private async Task<StoredProcedure> ReloadStoredProc(string sprocName)
		{

			var sproc = new StoredProcedure
			{
				Id = Path.GetFileNameWithoutExtension(sprocName),
				Body = File.ReadAllText(Path.Combine(this._serviceJSRoot, sprocName))
			};

			await TryDeleteStoredProcedure(this.Collection.SelfLink, sproc.Id);

			this._logger.WriteInformation("Creating stored procedure " + sproc.Id);
			this._logger.WriteVerbose("Creating stored procedure " + sproc.Body);
			sproc = await client.CreateStoredProcedureAsync(this.Collection.SelfLink, sproc);
			return sproc;

		}

		private  StoredProcedure LoadStoredProc(string name)
		{
			this._logger.WriteInformation("Loading sproc " + name + " from "+ collection.StoredProceduresLink);
			StoredProcedure sp = client.CreateStoredProcedureQuery(collection.StoredProceduresLink,
				"select * from root r where r.id = \"" + name + "\"").AsEnumerable().FirstOrDefault();

			return sp;
		}

		public async Task LoadStoredProcs(bool reload)
		{
			if (reload)
			{
				this._logger.WriteInformation("Reloading stored procs");
				this._addImageToBuild = await this.ReloadStoredProc(@"Services\js\addImageToBuild.js");
				this._saveBuild = await this.ReloadStoredProc(@"Services\js\saveBuild.js");
				this._saveList = await this.ReloadStoredProc(@"Services\js\saveList.js");
				this._addBuildToList = await this.ReloadStoredProc(@"Services\js\addBuildToList.js");
				this._removeBuildFromList = await this.ReloadStoredProc(@"Services\js\removeBuildFromList.js");
				this._removeImageFromBuild = await this.ReloadStoredProc(@"Services\js\removeImageFromBuild.js");
			}
			else
			{
				this._logger.WriteInformation("Loading stored procs from memory");
				this._addImageToBuild =  this.LoadStoredProc("addImageToBuild");
				this._saveBuild =  this.LoadStoredProc("saveBuild");
				this._saveList =  this.LoadStoredProc("saveList");
				this._addBuildToList =  this.LoadStoredProc("addBuildToList");
				this._removeBuildFromList =  this.LoadStoredProc("removeBuildFromList");
				this._removeImageFromBuild =  this.LoadStoredProc("removeImageFromBuild");
			}

		}

		private async Task TryDeleteStoredProcedure(string colSelfLink, string sprocId)
		{
			StoredProcedure sproc = Client.CreateStoredProcedureQuery(colSelfLink).Where(s => s.Id == sprocId).AsEnumerable().FirstOrDefault();
			if (sproc != null)
			{
				this._logger.WriteInformation("Deleting stored procedure " + sprocId);
				this._logger.WriteVerbose("Body:" + sproc.Body);
				await client.DeleteStoredProcedureAsync(sproc.SelfLink);
			}
		}

		public async Task AddImageToBuildAsync(string buildGuid, string imageGuid, string uid)
		{
			await EnsureStoredProcs();
			this._logger.WriteInformation("Executing stored procedure addImageToBuild(" + buildGuid + ", " + imageGuid + ", " + uid + ")");
			var response = await this.Client.ExecuteStoredProcedureAsync<string>(this._addImageToBuild.SelfLink, buildGuid, imageGuid, uid);
		}

		public async Task AddBuildToListAsync(string buildGuid, string listGuid, string uid)
		{
			await EnsureStoredProcs();
			this._logger.WriteInformation("Executing stored procedure addBuildToList(" + buildGuid + ", " + listGuid + ", " + uid + ")");
			var response = await this.Client.ExecuteStoredProcedureAsync<string>(this._addBuildToList.SelfLink, buildGuid, listGuid, uid);
		}
		public async Task RemoveBuildFromListAsync(string buildGuid, string listGuid, string uid)
		{
			await EnsureStoredProcs();
			this._logger.WriteInformation("Executing stored procedure removeBuildFromList(" + buildGuid + ", " + listGuid + ", " + uid + ")");
			var response = await this.Client.ExecuteStoredProcedureAsync<string>(this._removeBuildFromList.SelfLink, buildGuid, listGuid, uid);
		}

		public async Task DeleteImageFromBuildAsync(string buildGuid, string imageGuid, string uid)
		{
			await EnsureStoredProcs();

			this._logger.WriteInformation("Executing stored procedure DeleteImageFromBuild(" + buildGuid + ", " + imageGuid + ", " + uid + ")");
			var response = await this.Client.ExecuteStoredProcedureAsync<string>(this._removeImageFromBuild.SelfLink, buildGuid, imageGuid, uid);

			// todo/fixme ensure image is a part of build
			bool wasDeleted = await this._blobService.DeleteFile(this._imagesContainer, imageGuid);
			this._logger.WriteInformation("Deleted image " + imageGuid + ": " + wasDeleted.ToString());
		}

		public async Task<string> SaveBuildAsync(Build b, string uid)
		{
			await EnsureStoredProcs();
			var json = JsonConvert.SerializeObject(b);
			this._logger.WriteInformation("Executing stored procedure saveBuild(" + json + ", " + uid + ")");
			var response = await this.Client.ExecuteStoredProcedureAsync<string>(this._saveBuild.SelfLink, json, uid);
			return response;
		}

		public async Task<string> SaveListAsync(BuildList l, string uid)
		{
			await EnsureStoredProcs();
			var json = JsonConvert.SerializeObject(l);
			this._logger.WriteInformation("Executing stored procedure saveList(" + json + ", " + uid + ")");
			var response = await this.Client.ExecuteStoredProcedureAsync<string>(this._saveList.SelfLink, json, uid);
			return response;
		}


		public async Task DeleteBuildAsync(Build b, string uid)
		{
			if (b == null) {
				throw new InvalidDataException("Invalid build passed to DeleteBuild");
			}
			this._logger.WriteInformation("DeleteBuild id = " + b.id);
			var document = Client.CreateDocumentQuery(Collection.DocumentsLink)
						.Where(d => d.Id == b.id)
						.AsEnumerable()
						.First();
			var b2 = Client.CreateDocumentQuery<Build>(Collection.DocumentsLink)
						.Where(d => d.id == b.id)
						.AsEnumerable()
						.First();

			if (b.Creator != uid)
			{
				throw new Exception("User did not create this build");
			}
			
			//delete the images
			foreach (var i in b2.Images) {
				bool wasDeleted = await this._blobService.DeleteFile(this._imagesContainer, i.Guid);
				this._logger.WriteInformation("Deleted image " + i .Guid + ": " + wasDeleted.ToString());
			}

			await client.DeleteDocumentAsync(document.SelfLink);
        }

		public async Task DeleteListAsync(BuildList b, string uid)
		{
			if (b == null)
			{
				throw new InvalidDataException("Invalid build passed to DeleteList");
			}
			this._logger.WriteInformation("DeleteList id = " + b.id);
			var document = Client.CreateDocumentQuery(Collection.DocumentsLink)
						.Where(d => d.Id == b.id)
						.AsEnumerable()
						.First();
			var b2 = Client.CreateDocumentQuery<Build>(Collection.DocumentsLink)
						.Where(d => d.id == b.id)
						.AsEnumerable()
						.First();

			if (b.Creator != uid)
			{
				throw new Exception("User did not create this list");
			}

			await client.DeleteDocumentAsync(document.SelfLink);
		}

		public async Task<Document> CreateBuildAsync(Build item)
		{
			this._logger.WriteInformation("Creating build, id: " + item.id);
			return await Client.CreateDocumentAsync(Collection.SelfLink, item);
		}
		public async Task<Document> CreateListAsync(BuildList item)
		{
			this._logger.WriteInformation("Creating list, id: " + item.id);
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
		public BuildList GetList(string id)
		{
			this._logger.WriteInformation("GetList id = " + id);
			var b = Client.CreateDocumentQuery<BuildList>(Collection.DocumentsLink)
						.Where(d => d.id == id)
						.AsEnumerable()
						.FirstOrDefault();

			return b;
		}

		public async Task<IEnumerable<Build>> GetUserBuilds(string id)
		{
			this._logger.WriteInformation("GetUserBuilds = " + id);
			var b = await Task.Run(() => Client.CreateDocumentQuery<Build>(Collection.DocumentsLink)
						.Where(d => d.Creator == id && d.DocType == "build")
						.AsEnumerable().ToArray<Build>());

			return b;
		}
		public async Task<IEnumerable<BuildList>> GetUserLists(string id)
		{
			this._logger.WriteInformation("GetUserLists = " + id);
			var b = await Task.Run(() => Client.CreateDocumentQuery<BuildList>(Collection.DocumentsLink)
						.Where(d => d.Creator == id && d.DocType == "list")
						.AsEnumerable().ToArray<BuildList>());

			return b;
		}


	}
}