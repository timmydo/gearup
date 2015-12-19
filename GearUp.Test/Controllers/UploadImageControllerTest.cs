namespace GearUp.Test.Controllers
{
	using Auth;
	using GearUp.Controllers;
	using Interfaces;
	using Microsoft.AspNet.Http.Internal;
	using Microsoft.AspNet.Mvc;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Logging;
	using Microsoft.Extensions.OptionsModel;
	using Models;
	using Newtonsoft.Json.Linq;
	using Shared.Interfaces;
	using System;
	using System.Collections.Generic;
	using System.Security.Claims;
	using System.Threading.Tasks;
	using Xunit;

	public class UploadImageControllerTest
	{
		private readonly IServiceProvider _serviceProvider;

		public UploadImageControllerTest()
		{
			var services = new ServiceCollection();
			var lf = new LoggerFactory();
			services.AddScoped<IPartitionedKeyValueDictionary>((sp) => new MockDictionary());
			services.AddScoped<ILogger>((sp) => lf.CreateLogger(string.Empty));

			_serviceProvider = services.BuildServiceProvider();
		}

		private UploadImageController GetController()
		{
			var c = new UploadImageController(
				_serviceProvider.GetRequiredService<IOptions<SiteSettings>>(),
				_serviceProvider.GetRequiredService<ILogger>(),
				_serviceProvider.GetRequiredService<IPartitionedKeyValueDictionary>(),
				_serviceProvider.GetRequiredService<IAppBlobStorage>()
				);

			c.ControllerContext.HttpContext = new DefaultHttpContext();
			return c;
		}


		[Fact]
		public async Task BuildAddImage_BuildNotFound()
		{
			var c = GetController();
			var res = await c.Post("aoeu");
			Assert.False(true);
		}

		[Fact]
		public async Task BuildAddImage_NotOwner()
		{
			var c = GetController();
			var res = await c.Post("aoeu");

			Assert.False(true);
		}

		[Fact]
		public async Task BuildAddImage()
		{
			var c = GetController();
			var res = await c.Post("aoeu");
			Assert.False(true);
		}

	}
}
