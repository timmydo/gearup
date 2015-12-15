namespace GearUp.Fabric
{
	using System;
	using System.Fabric;
	using Microsoft.AspNet.Hosting;
	using Microsoft.Extensions.Configuration;
	using System.Diagnostics;
	using System.Threading;
#if true
	public class Program
	{

//		public static void Main(string[] args) => WebApplication.Run<Startup>(args);

		public static void Main(string[] args)
		{
			try
			{
				using (FabricRuntime fabricRuntime = FabricRuntime.Create())
				{
					//ServiceEventSource.Current.ServiceMessage()
					fabricRuntime.RegisterServiceType("FEType", typeof(WebService));
					ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(WebService).Name);

					Thread.Sleep(Timeout.Infinite);
				}
			}
			catch (Exception e)
			{
				ServiceEventSource.Current.ServiceHostInitializationFailed(e);
				throw;
			}
		}
	}
#endif
}
