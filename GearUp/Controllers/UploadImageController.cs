

namespace GearUp.Controllers
{
	using GearUp.Auth;
	using GearUp.Interfaces;
	using Microsoft.AspNet.Mvc;
	using Microsoft.Extensions.Logging;
	using Microsoft.Extensions.OptionsModel;
	using Models;
	using Services;
	using Shared.Interfaces;
	using System;
	using System.Linq;
	using System.Threading.Tasks;


	[Route("api/[controller]")]
	public class UploadImageController : Controller
	{

		public class UploadImageResult
		{
			public string Message { get; set; }
			public string Guid {get; set;}

		}


		private readonly IAppBlobStorage _blobService;
		private readonly ILogger _logger;
		private readonly IPartitionedKeyValueDictionary _ddb;
		private readonly string _imagesContainer;


		public UploadImageController(IOptions<SiteSettings> settings, ILogger logger, IPartitionedKeyValueDictionary ddb, IAppBlobStorage bs)
		{
			this._blobService = bs;
			this._logger = logger;
			this._ddb = ddb;
			this._imagesContainer = settings.Value.ImagesContainer;

		}

		public readonly string[] ValidContentTypes = { "image/png", "image/jpeg", "image/gif"};

		[HttpPost]
		[Produces("application/json", "text/json")]
		public async Task<UploadImageResult> Post([FromQuery]string buildid)
		{
			var stream = Request.Body;
			var result = new UploadImageResult()
			{
				Message = "Error",
				Guid = Guid.NewGuid().ToString()
			};

			if (!ValidContentTypes.Contains(Request.ContentType))
			{
				result.Message = "Invalid Content Type";
				return result;
			}

			if (string.IsNullOrEmpty(buildid))
			{
				result.Message = "Invalid Build ID";
				return result;
			}

			_logger.LogInformation("Upload Image, Content Type: " + Request.ContentType + " Build ID: " + buildid);

			// upload stream
			await this._blobService.UploadFile(stream, Request.ContentType, this._imagesContainer, result.Guid);
			result.Message = "Uploaded";

			// FIXME
			var uid = UserLogin.UserUniqueId(User?.Identity);
			//await this._ddb.AddImageToBuildAsync(buildid, result.Guid, uid);

			return result;

		}

	}
}
