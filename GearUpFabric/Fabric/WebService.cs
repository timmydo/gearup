

namespace GearUp.Fabric
{
	using Microsoft.ServiceFabric.Services.Client;
	using Microsoft.ServiceFabric.Services.Communication.Runtime;
	using Microsoft.ServiceFabric.Services.Runtime;
	using System;
	using System.Collections.Generic;
	using System.Fabric;
	using System.Fabric.Description;
	using System.Linq;
	using System.Threading.Tasks;
	using Microsoft.ServiceFabric.AspNet.Hosting;
	using Microsoft.AspNet.Hosting;


#if true
	public class WebService : StatelessService
    {
		private readonly ServicePartitionResolver servicePartitionResolver = ServicePartitionResolver.GetDefault();

		protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
		{
			return new[] { new ServiceInstanceListener(this.CreateInputListener, "Input") };
		}
		private ICommunicationListener CreateInputListener(StatelessServiceInitializationParameters args)
		{
			EndpointResourceDescription inputEndpoint = args.CodePackageActivationContext.GetEndpoint("FEEndpoint");
			string uriPrefix = String.Format("{0}://+:{1}/", inputEndpoint.Protocol, inputEndpoint.Port);
			string uriPublished = uriPrefix.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);

			return new HttpCommunicationListener(uriPrefix, uriPublished, this);
		}

	}
#endif
}
