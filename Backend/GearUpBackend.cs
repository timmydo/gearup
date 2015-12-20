﻿using Microsoft.ServiceFabric.Data.Collections;
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
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Shared.Interfaces;

namespace GearUpBackend
{
	/// <summary>
	/// The FabricRuntime creates an instance of this class for each service type instance.
	/// </summary>
	public class GearUpBackend : StatefulService, IPartitionedKeyValueDictionary, IService
	{
		private IReliableDictionary<string, string> _dict;

		public async Task AddKeyAsync(string key, string value)
		{
			if (_dict == null) throw new NullReferenceException(nameof(_dict));
			using (var tx = this.StateManager.CreateTransaction())
			{
				await _dict.AddAsync(tx, key, value);
				await tx.CommitAsync();
			}
		}

		public async Task DeleteKeyAsync(string key)
		{
			if (_dict == null) throw new NullReferenceException(nameof(_dict));
			using (var tx = this.StateManager.CreateTransaction())
			{
				await _dict.TryRemoveAsync(tx, key);
				await tx.CommitAsync();
			}
		}

		public async Task<string> GetKeyAsync(string key)
		{
			if (_dict == null) throw new NullReferenceException(nameof(_dict));
			using (var tx = this.StateManager.CreateTransaction())
			{
				var result = await _dict.TryGetValueAsync(tx, key);
				return result.HasValue ? result.Value : string.Empty;
			}
		}

		public async Task<bool> UpdateKeyAsync(string key, string value, string updateFunc)
		{
			if (_dict == null) throw new NullReferenceException(nameof(_dict));
			if (updateFunc == null) updateFunc = string.Empty;
			using (var tx = this.StateManager.CreateTransaction())
			{
				switch (updateFunc)
				{
					case "overwrite":
					default:
						await _dict.AddOrUpdateAsync(tx, key, value, (k, v) => value);
						break;
				}

				await tx.CommitAsync();
			}

			return true;
		}

		/// <summary>
		/// Optional override to create listeners (like tcp, http) for this service replica.
		/// </summary>
		/// <returns>The collection of listeners.</returns>
		protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
		{
			return new[] { new ServiceReplicaListener(parameters => new ServiceRemotingListener<GearUpBackend>(parameters, this)) };
		}

		/// <summary>
		/// This is the main entry point for your service's partition replica. 
		/// RunAsync executes when the primary replica for this partition has write status.
		/// </summary>
		/// <param name="cancelServicePartitionReplica">Canceled when Service Fabric terminates this partition's replica.</param>
		protected override async Task RunAsync(CancellationToken cancelServicePartitionReplica)
		{
			this._dict = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, string>>("myDictionary");
		}
	}
}
