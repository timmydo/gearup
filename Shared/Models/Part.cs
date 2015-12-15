namespace GearUp.Models
{
	using System.Runtime.Serialization;

	[DataContract]
	public class Part
    {
		//public string Url { get; set; }
		//public string Price { get; set; }
		public string Title { get; set; }

		public Part()
		{
			Title = "New item";
		}
	}
}