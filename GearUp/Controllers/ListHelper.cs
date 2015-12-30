namespace GearUp.Controllers
{
	using Newtonsoft.Json;
	using Shared.Interfaces;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;


	public class ListHelper
    {
		public static async Task AddToList(IPartitionedKeyValueDictionary data, string ns, string uid, string idToAdd)
		{
			var ub = await data.GetKeyAsync(ns + uid);
			IList<string> userList;
			if (ub == null)
			{
				userList = new List<string>() { idToAdd };
				await data.AddKeyAsync(ns + uid,
				JsonConvert.SerializeObject(userList));
			}
			else
			{
				userList = JsonConvert.DeserializeObject<List<string>>(ub.Value);
				userList.Add(idToAdd);
				ub.Value = JsonConvert.SerializeObject(userList);
				await ub.UpdateAsync();
			}
		}

		public static async Task EnqueueList(IPartitionedKeyValueDictionary data, string ns, string uid, string idToAdd)
		{
			const int maxSize = 1000;
			var ub = await data.GetKeyAsync(ns + uid);
			IList<string> q;
			if (ub == null)
			{
				q = new List<string>() { idToAdd };
				await data.AddKeyAsync(ns + uid, JsonConvert.SerializeObject(q));
			}
			else
			{
				q = JsonConvert.DeserializeObject<List<string>>(ub.Value);
				if (!q.Contains(idToAdd))
				{
					q.Insert(0, idToAdd);
					if (q.Count > maxSize)
					{
						q = q.Take(maxSize).ToList();
					}
					ub.Value = JsonConvert.SerializeObject(q);
					await ub.UpdateAsync();
				}
			}
		}

		public static async Task RemoveFromList(IPartitionedKeyValueDictionary data, string ns, string uid, string idToRemove)
		{
			var ub = await data.GetKeyAsync(ns + uid);
			IList<string> userBuilds;
			if (ub == null)
			{
				userBuilds = new List<string>();
			}
			else
			{
				userBuilds = JsonConvert.DeserializeObject<List<string>>(ub.Value);
			}

			userBuilds.Remove(idToRemove);
			ub.Value = JsonConvert.SerializeObject(userBuilds);
			await ub.UpdateAsync();
		}

		public static async Task RemoveFromQueue(IPartitionedKeyValueDictionary data, string ns, string uid, string idToRemove)
		{
			var ub = await data.GetKeyAsync(ns + uid);
			IList<string> q;
			if (ub == null)
			{
				q = new List<string>();
			}
			else
			{
				q = JsonConvert.DeserializeObject<List<string>>(ub.Value);
			}

			if (q.Contains(idToRemove))
			{
				q.Remove(idToRemove);
				ub.Value = JsonConvert.SerializeObject(q);
				await ub.UpdateAsync();
			}
		}
	}
}
