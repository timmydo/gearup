using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GearUp.Models
{
	[DataContract]
	public class Build
	{
		public const string CreatorField = "Creator";

		public Build()
		{
			Version = 1;
			id = System.Guid.NewGuid().ToString();
			Modified = DateTime.UtcNow;
			Created = Modified;
			Creator = string.Empty;
			DocType = "build";
			Title = "";
			Description = "";
			Url = string.Empty;
			Images = new List<Image>();
			Parts = new List<Part>(); // guid ref

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
		public int LowTemperature { get; set; }
		public int HighTemperature { get; set; }
		public bool IsCelsius { get; set; }

		public List<Image> Images { get; set; }

		public List<Part> Parts { get; set; }
	}
}