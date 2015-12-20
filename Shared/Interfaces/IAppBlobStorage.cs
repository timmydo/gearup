using System.Threading.Tasks;
using System.IO;

namespace GearUp.Interfaces
{
    public interface IAppBlobStorage
    {
		Task<string> UploadUserImage(Stream stream, string contentType);
		Task<bool> DeleteFile(string containerName, string uid);

    }

}