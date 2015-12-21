namespace GearUp.Test.Mocks
{
	using GearUp.Interfaces;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.IO;

	public class MockBlobStorage : IAppBlobStorage
	{
		private HashSet<string> _images = new HashSet<string>();

		public async Task<bool> DeleteImage(string uid)
		{
			if (_images.Contains(uid))
			{
				_images.Remove(uid);
				return await Task.FromResult(true);
			}
			else
			{
				return await Task.FromResult(false);
			}
		}

		public async Task<string> UploadUserImage(Stream stream, string contentType)
		{
			var uid = Guid.NewGuid().ToString("N");
			_images.Add(uid);
			return await Task.FromResult(uid);
		}
	}
}
