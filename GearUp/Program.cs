namespace GearUp
{
	using System;
	using Microsoft.AspNet.Hosting;
	using GearUp;

	public class Program
	{
		public static void Main(string[] args)
		{
			var config = WebApplicationConfiguration.GetDefault(args);

			var application = new WebApplicationBuilder()
				.UseConfiguration(config)
				.UseStartup<Startup>()
				.Build();

			using (application.Start())
			{
				Console.ReadLine();
			}
		}
	}
}
