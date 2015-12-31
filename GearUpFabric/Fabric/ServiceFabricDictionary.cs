namespace GearUpFabric.Fabric
{
	using Microsoft.ServiceFabric.Services.Remoting;
	using Shared.Interfaces;

	public interface IServiceFabricDictionary : IPartitionedKeyValueDictionary, IService
	{
    }
}
