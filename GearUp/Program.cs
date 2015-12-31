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

			string port = System.Environment.GetEnvironmentVariable("HTTP_PLATFORM_PORT");
			Console.WriteLine("Port: " + (string.IsNullOrEmpty(port) ? "Not set" : port));

			using (application.Start())
			{
				Console.ReadLine();
			}

			Console.WriteLine("Exiting...");
		}
	}
}
