namespace GearUp
{
	using System;
	using Microsoft.AspNet.Hosting;
	using System.Linq;

	public class Program
	{
		public static void Main(string[] args)
		{
			Console.WriteLine("Args:\n" + string.Join("\n", args));
			var config = WebApplicationConfiguration.GetDefault(args);

			var application = new WebApplicationBuilder()
				.UseConfiguration(config)
				.UseStartup<Startup>()
				.Build();

			var addresses = application.GetAddresses();
			Console.WriteLine("Listening on " + string.Join(", ", addresses));

			try
			{
				application.Run();
			}
			finally
			{
				Console.WriteLine("Exiting...");
			}
		}
	}
}
