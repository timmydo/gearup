namespace GearUp.Models
{
	using System;
	using System.Runtime.Serialization;
	using System.Collections.Generic;

	[DataContract]
	public class BuildList
	{
		public BuildList()
		{
			Version = 1;
			id = System.Guid.NewGuid().ToString();
			Modified = DateTime.UtcNow;
			Created = Modified;
			Creator = string.Empty;
			DocType = "list";
			Title = "";
			Description = "";
			Url = string.Empty;
			Builds = new List<string>(); // guid ref

		}

		public int Version { get; set; }
		public string id { get; set; }
		public string DocType { get; set; }
		public DateTime Modified { get; set; }
		public DateTime Created { get; set; }
		public string Creator { get; set; }

		public string Title { get; set; }
		public string Description { get; set; }

		public string Url { get; set; }


		public List<string> Builds { get; set; }
	}
}