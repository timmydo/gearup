namespace GearUp.Controllers
{
	using Newtonsoft.Json;
	using Shared.Interfaces;
	using System.Collections.Generic;
	using System.Threading.Tasks;


	public class ListHelper
    {
		public static async Task AddToList(IPartitionedKeyValueDictionary data, string ns, string uid, string idToAdd)
		{
			var ub = await data.GetKeyAsync(ns + uid);
			IList<string> userList;
			if (string.IsNullOrEmpty(ub))
			{
				userList = new List<string>() { idToAdd };
				await data.AddKeyAsync(ns + uid,
				JsonConvert.SerializeObject(userList));
			}
			else
			{
				userList = JsonConvert.DeserializeObject<List<string>>(ub);
				userList.Add(idToAdd);
				await data.UpdateKeyAsync(ns + uid,	JsonConvert.SerializeObject(userList));
			}
		}

		public static async Task RemoveFromList(IPartitionedKeyValueDictionary data, string ns, string uid, string idToRemove)
		{
			var ub = await data.GetKeyAsync(ns + uid);
			IList<string> userBuilds;
			if (string.IsNullOrEmpty(ub))
			{
				userBuilds = new List<string>();
			}
			else
			{
				userBuilds = JsonConvert.DeserializeObject<List<string>>(ub);
			}

			userBuilds.Remove(idToRemove);
			await data.UpdateKeyAsync(ns + uid,	JsonConvert.SerializeObject(userBuilds));
		}
    }
}
