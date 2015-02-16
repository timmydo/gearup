namespace GearUp.Models
{
	public class Image
	{
		public string Guid { get; set;  }

		public Image()
		{
			Guid = System.Guid.NewGuid().ToString();
		}

	}
}