namespace GearUp.Fabric
{
	using Microsoft.ServiceFabric.Services.Communication.Runtime;
	using Microsoft.AspNet.Server.WebListener;
	using System;
	using System.Threading.Tasks;
	using System.Threading;

#if false
	public sealed class HttpCommunicationListener : ICommunicationListener
	{
		private readonly string publishUri;
		private readonly WebListener listener;
		private readonly Func<HttpListenerContext, CancellationToken, Task> processRequest;
		private readonly CancellationTokenSource processRequestsCancellation = new CancellationTokenSource();
		public HttpCommunicationListener(string uriPrefix, string uriPublished, Func<HttpListenerContext, CancellationToken, Task> processRequest)
		{
			this.publishUri = uriPublished;
			this.processRequest = processRequest;
			this.httpListener = new Microsoft.AspNet.Server.WebListener.ServerFactory();
			this.httpListener.Prefixes.Add(uriPrefix);
		}

		public void Abort()
		{
			this.processRequestsCancellation.Cancel();
			this.httpListener.Abort();
		}

		public Task CloseAsync(CancellationToken cancellationToken)
		{
			this.processRequestsCancellation.Cancel();
			this.httpListener.Close();
			return Task.FromResult(true);
		}

		public Task<string> OpenAsync(CancellationToken cancellationToken)
		{
			this.httpListener.Start();

			Task openTask = this.ProcessRequestsAsync(this.processRequestsCancellation.Token);

			return Task.FromResult(this.publishUri);
		}

		private async Task ProcessRequestsAsync(CancellationToken processRequests)
		{
			while (!processRequests.IsCancellationRequested)
			{
				HttpListenerContext request = await this.httpListener.GetContextAsync();

				// The ContinueWith forces rethrowing the exception if the task fails.
				Task requestTask = this.processRequest(request, this.processRequestsCancellation.Token)
					.ContinueWith(async t => await t /* Rethrow unhandled exception */, TaskContinuationOptions.OnlyOnFaulted);
			}
		}
	}
#endif
}
