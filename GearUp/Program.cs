namespace GearUp
{
	using System;
	using Microsoft.AspNet.Hosting;
	using Microsoft.Extensions.Configuration;

	public class Program
	{
		public static void Main(string[] args)
		{
			//Add command line configuration source to read command line parameters.
			var builder = new ConfigurationBuilder();
			builder.AddCommandLine(args);
			var config = builder.Build();

			using (new WebHostBuilder(config)
				.UseServerFactory("Microsoft.AspNet.Server.WebListener")
				.Build()
				.Start())
			{
				Console.WriteLine("Started the server..");
				Console.WriteLine("Press any key to stop the server");
				Console.ReadLine();
			}
		}
	}
}
