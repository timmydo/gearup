﻿
namespace GearUp.Models
{
	using System.Runtime.Serialization;

	[DataContract]
	public class Image
	{
		public string Guid { get; set;  }
		public string Title { get; set; }

		public Image()
		{
			Guid = System.Guid.NewGuid().ToString();
		}

	}
}