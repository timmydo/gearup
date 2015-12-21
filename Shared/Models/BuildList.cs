namespace GearUp.Models
{
	using System;
	using System.Runtime.Serialization;
	using System.Collections.Generic;

	public class BuildList
	{
		public string Id { get; set; }
		public DateTime Modified { get; set; }
		public DateTime Created { get; set; }
		public string Creator { get; set; }

		public string Title { get; set; }
		public string Description { get; set; }

		public IList<string> Builds { get; set; }
	}
}