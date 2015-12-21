using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GearUp.Models
{
	public class Build
	{
		public string Id { get; set; }
		public DateTime Modified { get; set; }
		public DateTime Created { get; set; }
		public string Creator { get; set; }

		public string Title { get; set; }
		public string Description { get; set; }

		public int LowTemperature { get; set; }
		public int HighTemperature { get; set; }
		public bool IsCelsius { get; set; }

		public IList<Image> Images { get; set; }

		public IList<Part> Parts { get; set; }
	}
}