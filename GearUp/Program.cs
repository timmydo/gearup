namespace GearUp
{
	using System;
	using Microsoft.AspNet.Hosting;
	using GearUp;

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

			string port = System.Environment.GetEnvironmentVariable("HTTP_PLATFORM_PORT");
			if (string.IsNullOrEmpty(port))
			{
				port = "5000";
			}

			Console.WriteLine("Port: " + port);

			var addresses = application.GetAddresses();
			addresses.Clear();
			addresses.Add("http://localhost:" + port);

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
