namespace GearUp.Fabric
{
	using Microsoft.ServiceFabric.Services.Communication.Runtime;
	using Microsoft.AspNet.Hosting;
	using System;
	using System.Threading.Tasks;
	using System.Threading;
	using Microsoft.AspNet.Hosting.Internal;
	using Microsoft.Extensions.Configuration;
	using Microsoft.ServiceFabric.Services.Runtime;
	using Microsoft.Extensions.DependencyInjection;
#if true
	public sealed class HttpCommunicationListener : ICommunicationListener
	{
		private readonly string publishUri;
		private readonly CancellationTokenSource processRequestsCancellation = new CancellationTokenSource();
		private IHostingEngine engine;
		private IApplication application;
		private StatelessService svc;

		public HttpCommunicationListener(string uriPrefix, string uriPublished, StatelessService svc)
		{
			this.svc = svc;
			ServiceEventSource.Current.ServiceMessage(this.svc, "Building config " + uriPrefix);

			var config = new ConfigurationBuilder()
				.AddJsonFile("config.json")
				.AddCommandLine(new string[] { "--server", "Microsoft.AspNet.Server.WebListener", "--server.urls", uriPrefix })
				.AddEnvironmentVariables().Build();

			ServiceEventSource.Current.ServiceMessage(this.svc, "Building engine");
			this.engine = new WebHostBuilder(config).UseStartup(typeof(GearUp.Web.Startup)).Build();

			ServiceEventSource.Current.ServiceMessage(this.svc, "starting");
			this.application = engine.Start();

			ServiceEventSource.Current.ServiceMessage(this.svc, "started");
			this.publishUri = uriPublished;
		}

		public void Abort()
		{
			ServiceEventSource.Current.ServiceMessage(this.svc, "abort");
			this.processRequestsCancellation.Cancel();
			CloseApp();
			this.application.Dispose();
		}

		private void CloseApp()
		{
			var appLifetime = this.application.Services.GetRequiredService<IApplicationLifetime>();
			appLifetime.StopApplication();
			appLifetime.ApplicationStopping.WaitHandle.WaitOne();
		}

		public Task CloseAsync(CancellationToken cancellationToken)
		{
			ServiceEventSource.Current.ServiceMessage(this.svc, "closeasync");
			this.processRequestsCancellation.Cancel();
			CloseApp();
			return Task.FromResult(true);
		}

		public Task<string> OpenAsync(CancellationToken cancellationToken)
		{
			ServiceEventSource.Current.ServiceMessage(this.svc, "openasync");
			Task openTask = this.ProcessRequestsAsync(this.processRequestsCancellation.Token);

			return Task.FromResult(this.publishUri);
		}

		private async Task ProcessRequestsAsync(CancellationToken processRequests)
		{
			ServiceEventSource.Current.ServiceMessage(this.svc, "processrequest");
			while (!processRequests.IsCancellationRequested)
			{
				await Task.Delay(1000);
			}
		}
	}
#endif
}
