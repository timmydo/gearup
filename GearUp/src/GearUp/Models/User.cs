using System;

namespace GearUp.Models
{
    public class User
    {
		public string Guid { get; set; }
		public string Title { get; set; }

		public User()
		{
			Guid = System.Guid.Empty.ToString();
			Title = string.Empty;
		}
	}
}