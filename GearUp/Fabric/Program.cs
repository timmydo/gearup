namespace GearUp.Web
{
	using System;
	using System.Fabric;
	using Microsoft.AspNet.Hosting;
	using Microsoft.Extensions.Configuration;
	using System.Diagnostics;
	using System.Threading;
	public class Program
	{

#if true
		public static void Main(string[] args) => WebApplication.Run<Startup>(args);

#else
		public static void Main(string[] args)
		{
			try
			{
				using (FabricRuntime fabricRuntime = FabricRuntime.Create())
				{
					// This is the name of the ServiceType that is registered with FabricRuntime. 
					// This name must match the name defined in the ServiceManifest. If you change
					// this name, please change the name of the ServiceType in the ServiceManifest.
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
#endif
	}
}
