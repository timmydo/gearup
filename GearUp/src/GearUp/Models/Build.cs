using System;
using System.Collections.Generic;

namespace GearUp.Models
{
    public class Build
    {
		public Build()
		{
			Version = 1;
			id = System.Guid.NewGuid().ToString();
			Modified = DateTime.UtcNow;
			Created = Modified;
			Creator = new User(); //fixme set user guid
			Title = "New Build";
			Description = "New Build";
			Url = string.Empty;
			Images = new List<Image>();
			Parts = new List<Part>(); // guid ref

		}

		public int Version { get; set; }
		public string id { get; set; }
		public DateTime Modified { get; set; }
		public DateTime Created { get; set; }
		public User Creator { get; set; }

		public string Title { get; set; }
		public string Description { get; set; }

		public string Url { get; set; }

		public List<Image> Images { get; set; }

		public List<Part> Parts { get; set; }
	}
}