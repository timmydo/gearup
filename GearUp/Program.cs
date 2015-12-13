﻿namespace GearUp
{
	using System;
	using System.Fabric;
	using Microsoft.AspNet.Hosting;
	using Microsoft.Extensions.Configuration;
	using System.Diagnostics;
	using System.Threading;
	public class Program
	{
		public static void Main(string[] args)
		{
			try
			{
				using (FabricRuntime fabricRuntime = FabricRuntime.Create())
				{
					// This is the name of the ServiceType that is registered with FabricRuntime. 
					// This name must match the name defined in the ServiceManifest. If you change
					// this name, please change the name of the ServiceType in the ServiceManifest.
					fabricRuntime.RegisterServiceType("WebType", typeof(Web));

					ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(Web).Name);

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
}
