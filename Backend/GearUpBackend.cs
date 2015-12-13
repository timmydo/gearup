using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GearUp.Interfaces;
using GearUp.Models;
using Microsoft.ServiceFabric.Services.Remoting;

namespace GearUpBackend
{
	/// <summary>
	/// The FabricRuntime creates an instance of this class for each service type instance.
	/// </summary>
	public class GearUpBackend : StatefulService, IAppDataService, IService
	{
		public Task AddBuildToListAsync(string buildGuid, string listGuid, string uid)
		{
			throw new NotImplementedException();
		}

		public Task AddImageToBuildAsync(string buildGuid, string imageGuid, string uid)
		{
			throw new NotImplementedException();
		}

		public Task CreateBuildAsync(Build item)
		{
			throw new NotImplementedException();
		}

		public Task CreateListAsync(BuildList item)
		{
			throw new NotImplementedException();
		}

		public Task DeleteBuildAsync(Build b, string uid)
		{
			throw new NotImplementedException();
		}

		public Task DeleteImageFromBuildAsync(string buildGuid, string imageGuid, string uid)
		{
			throw new NotImplementedException();
		}

		public Task DeleteListAsync(BuildList b, string uid)
		{
			throw new NotImplementedException();
		}

		public Task<Build> GetBuildAsync(string id)
		{
			throw new NotImplementedException();
		}

		public Task<BuildList> GetListAsync(string id)
		{
			throw new NotImplementedException();
		}

		public Task<string[]> GetRecentBuildsAsync()
		{
			return Task.FromResult(new string[0]);
		}

		public Task<Build[]> GetUserBuildsAsync(string id)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<BuildList>> GetUserListsAsync(string id)
		{
			throw new NotImplementedException();
		}

		public Task RemoveBuildFromListAsync(string buildGuid, string listGuid, string uid)
		{
			throw new NotImplementedException();
		}

		public Task<string> SaveBuildAsync(Build b, string uid)
		{
			throw new NotImplementedException();
		}

		public Task<string> SaveListAsync(BuildList l, string uid)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Optional override to create listeners (like tcp, http) for this service replica.
		/// </summary>
		/// <returns>The collection of listeners.</returns>
		protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
		{
			// TODO: If your service needs to handle user requests, return a list of ServiceReplicaListeners here.
			return new ServiceReplicaListener[0];
		}

		/// <summary>
		/// This is the main entry point for your service's partition replica. 
		/// RunAsync executes when the primary replica for this partition has write status.
		/// </summary>
		/// <param name="cancelServicePartitionReplica">Canceled when Service Fabric terminates this partition's replica.</param>
		protected override async Task RunAsync(CancellationToken cancelServicePartitionReplica)
		{
			// TODO: Replace the following sample code with your own logic.

			// Gets (or creates) a replicated dictionary called "myDictionary" in this partition.
			var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");

			// This partition's replica continues processing until the replica is terminated.
			while (!cancelServicePartitionReplica.IsCancellationRequested)
			{

				// Create a transaction to perform operations on data within this partition's replica.
				using (var tx = this.StateManager.CreateTransaction())
				{

					// Try to read a value from the dictionary whose key is "Counter-1".
					var result = await myDictionary.TryGetValueAsync(tx, "Counter-1");

					// Log whether the value existed or not.
					ServiceEventSource.Current.ServiceMessage(this, "Current Counter Value: {0}",
						result.HasValue ? result.Value.ToString() : "Value does not exist.");

					// If the "Counter-1" key doesn't exist, set its value to 0
					// else add 1 to its current value.
					await myDictionary.AddOrUpdateAsync(tx, "Counter-1", 0, (k, v) => ++v);

					// Committing the transaction serializes the changes and writes them to this partition's secondary replicas.
					// If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
					// discarded, and nothing is sent to this partition's secondary replicas.
					await tx.CommitAsync();
				}

				// Pause for 1 second before continue processing.
				await Task.Delay(TimeSpan.FromSeconds(1), cancelServicePartitionReplica);
			}
		}
	}
}
