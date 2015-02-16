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
			Modifed = DateTime.UtcNow;
			Created = Modifed;
			Creator = new User();
			Title = "New Build";
			Description = "New Build";
			Main = new Anchor();
			Reseller = new List<Anchor>();
			Price = "Unknown";
			Images = new List<Image>();
			parts = new List<string>(); // guid ref

		}

		public int Version { get; set; }
		public string id { get; set; }
		public DateTime Modifed { get; set; }
		public DateTime Created { get; set; }
		public User Creator { get; set; }

		public string Title { get; set; }
		public string Description { get; set; }

		public Anchor Main { get; set; }
		public List<Anchor> Reseller { get; set; }

		public string Price { get; set; }

		public List<Image> Images { get; set; }

		public List<string> parts { get; set; }
	}
}