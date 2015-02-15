using System;
using System.Collections.Generic;

namespace GearUp.Models
{
    public class Build
    {
		public int Version { get; set; }
		public string Id { get; set; }
		public long Modifed { get; set; }
		public long Created { get; set; }
		public User Creator { get; set; }

		public string Title { get; set; }
		public string Description { get; set; }

		public Anchor Main { get; set; }
		public Anchor Reseller { get; set; }

		public string Price { get; set; }

		public List<Image> Images { get; set; }

		public List<Build> parts { get; set; } 


	}
}