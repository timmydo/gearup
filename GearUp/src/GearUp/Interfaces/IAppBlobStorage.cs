using System.Threading.Tasks;
using System.IO;

namespace GearUp.Interfaces
{
    public interface IAppBlobStorage
    {
		Task UploadFile(Stream stream, string contentType, string containerName, string uid);
		Task<bool> DeleteFile(string containerName, string uid);

    }

}