
namespace GearUp.Test.Controllers
{
	using Auth;
	using GearUp.Controllers;
	using Interfaces;
	using Microsoft.AspNet.Http.Internal;
	using Microsoft.AspNet.Mvc;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Logging;
	using Mocks;
	using Models;
	using Newtonsoft.Json.Linq;
	using Shared.Interfaces;
	using System;
	using System.Collections.Generic;
	using System.Security.Claims;
	using System.Threading.Tasks;
	using System.Linq;
	using Xunit;
	using Newtonsoft.Json;
	public class BuildControllerTest
	{
		private readonly IServiceProvider _serviceProvider;

		public BuildControllerTest()
		{
			var services = new ServiceCollection();
			var lf = new LoggerFactory();
			services.AddScoped<IPartitionedKeyValueDictionary>((sp) => new MockDictionary());
			services.AddScoped<ILogger>((sp) => lf.CreateLogger(string.Empty));
			services.AddScoped<IAppBlobStorage>((sp) => new MockBlobStorage());

			_serviceProvider = services.BuildServiceProvider();
		}

		public BuildController GetController()
		{
			var bc = new BuildController(_serviceProvider.GetRequiredService<ILogger>(),
				_serviceProvider.GetRequiredService<IPartitionedKeyValueDictionary>(),
				_serviceProvider.GetRequiredService<IAppBlobStorage>()
				);

			bc.ControllerContext.HttpContext = new DefaultHttpContext();
			return bc;
		}

		[Fact]
		public async Task GetBuild_NotFound()
		{
			var c = GetController();

			var result = await c.GetById("123");

			Assert.True(c.HttpContext.Response.StatusCode == 404);
			Assert.True(result == null);
		}

		[Fact]
		public async Task CreateBuild_MustLogIn()
		{
			var c = GetController();
			c.HttpContext.User = new ClaimsPrincipal();
			var b = await c.CreateBuild();

			Assert.True(c.HttpContext.Response.StatusCode == 401);
			Assert.True(b == null);
		}

		[Fact]
		public async Task CreateBuild_GetAndDeleteBuild()
		{
			var c = GetController();

			TestHelper.SetupUser(c);
			
			var uuid = UserLogin.UserUniqueId(c.HttpContext.User.Identity);
			var build = await c.CreateBuild();

			Assert.True(c.HttpContext.Response.StatusCode == 200);
			Assert.True(build != null);

			var result2 = await c.GetById(build.Id);

			Assert.True(c.HttpContext.Response.StatusCode == 200);
			Assert.True(result2 != null);
			Assert.True(result2.Id == build.Id);
			Assert.True(result2.Creator == uuid);
			Assert.True(result2.Creator == build.Creator);

			await c.DeleteBuild(build.Id);
			Assert.True(c.HttpContext.Response.StatusCode == 200);

			var result3 = await c.GetById(build.Id);

			Assert.True(c.HttpContext.Response.StatusCode == 404);
			Assert.True(result3 == null);
		}



		[Fact]
		public async Task DeleteBuild_NotFound()
		{
			var c = GetController();
			await c.DeleteBuild("123");
			Assert.True(c.HttpContext.Response.StatusCode == 404);
		}

		[Fact]
		public async Task DeleteBuild_NotOwner()
		{
			var c = GetController();
			TestHelper.SetupUser(c, "1");
			var result = await c.CreateBuild();
			TestHelper.SetupUser(c, "2");
			await c.DeleteBuild(result.Id);
			Assert.True(c.HttpContext.Response.StatusCode == 403);
		}

		[Fact]
		public async Task Build_GetMultiple()
		{
			var c = GetController();
			TestHelper.SetupUser(c);
			var result = await c.CreateBuild();
			Assert.True(c.HttpContext.Response.StatusCode == 200);

			var result2 = await c.CreateBuild();
			Assert.True(c.HttpContext.Response.StatusCode == 200);

			var bl = new List<string>()
			{
				result.Id,
				result2.Id
			};

			var result3 = await c.GetMultiple(bl);
			Assert.True(c.HttpContext.Response.StatusCode == 200);
			Assert.True(result3[0].Id.Equals(result.Id));
			Assert.True(result3[1].Id.Equals(result2.Id));
		}

		[Fact]
		public async Task Build_GetMultiple_EmptyList()
		{
			var c = GetController();

			var bl = new List<string>()
			{
			};

			var result3 = await c.GetMultiple(bl);
			Assert.True(c.HttpContext.Response.StatusCode == 200);
			Assert.True(result3.Count == 0);
		}

		[Fact]
		public async Task Build_GetMultiple_NotExist()
		{
			var c = GetController();

			var bl = new List<string>()
			{
				"abc"
			};

			var result3 = await c.GetMultiple(bl);
			Assert.True(c.HttpContext.Response.StatusCode == 200);
			Assert.True(result3.Count == 0);
		}

		[Fact]
		public async Task BuildAddImage_DeleteImage_Success()
		{
			var c = GetController();
			TestHelper.SetupUser(c);
			var bresult = (await c.CreateBuild()).Id;
			Assert.True(c.HttpContext.Response.StatusCode == 200);

			c.Request.HttpContext.Request.ContentType = c.ValidContentTypes[0];
			var iresult = await c.AddImage(bresult);
			Assert.True(c.HttpContext.Response.StatusCode == 200);

			// check to see if the image exists
			var newb = await c.GetById(bresult);
			Assert.True(newb.Images[0].Id == iresult);

			var dpi = new BuildController.DeleteImageParamInfo
			{
				Build = bresult,
				Image = iresult
			};

			var dresult = await c.DeleteImage(dpi);
			Assert.True(c.HttpContext.Response.StatusCode == 200);
		}

		[Fact]
		public async Task BuildAddImage_BuildNotFound()
		{
			var c = GetController();
			TestHelper.SetupUser(c);
			c.Request.HttpContext.Request.ContentType = c.ValidContentTypes[0];
			var result = await c.AddImage("abc");

			Assert.True(c.HttpContext.Response.StatusCode == 404);
		}

		[Fact]
		public async Task BuildAddImage_NotOwner()
		{
			var c = GetController();
			TestHelper.SetupUser(c, "1");
			var bresult = (await c.CreateBuild()).Id;
			c.Request.HttpContext.Request.ContentType = c.ValidContentTypes[0];
			TestHelper.SetupUser(c, "2");
			var iresult = await c.AddImage(bresult);

			Assert.True(c.HttpContext.Response.StatusCode == 403);
		}

		[Fact]
		public async Task BuildAddImage_BadContentType()
		{
			var c = GetController();
			TestHelper.SetupUser(c);
			var bresult = (await c.CreateBuild()).Id;
			c.Request.HttpContext.Request.ContentType = "asdf";
			var result = await c.AddImage(bresult);
			Assert.True(c.HttpContext.Response.StatusCode == 400);
		}

		[Fact]
		public async Task BuildDeleteImage_NotOwner()
		{
			var c = GetController();
			TestHelper.SetupUser(c);
			var bresult = (await c.CreateBuild()).Id;
			Assert.True(c.HttpContext.Response.StatusCode == 200);

			c.Request.HttpContext.Request.ContentType = c.ValidContentTypes[0];
			var iresult = await c.AddImage(bresult);
			Assert.True(c.HttpContext.Response.StatusCode == 200);

			var dpi = new BuildController.DeleteImageParamInfo
			{
				Build = bresult,
				Image = iresult
			};
			TestHelper.SetupUser(c, "abc");
			var dresult = await c.DeleteImage(dpi);
			Assert.True(c.HttpContext.Response.StatusCode == 403);
		}

		[Fact]
		public async Task BuildDeleteImage_BadImage()
		{
			var c = GetController();
			TestHelper.SetupUser(c);
			var bresult = (await c.CreateBuild()).Id;
			c.Request.HttpContext.Request.ContentType = c.ValidContentTypes[0];

			var dpi = new BuildController.DeleteImageParamInfo
			{
				Build = bresult,
				Image = "asdf"
			};

			var dresult = await c.DeleteImage(dpi);
			Assert.True(c.HttpContext.Response.StatusCode == 400);
		}

		[Fact]
		public async Task BuildDeleteImage_BadBuild()
		{
			var c = GetController();
			TestHelper.SetupUser(c);
			var dpi = new BuildController.DeleteImageParamInfo
			{
				Build = "asdf",
				Image = "asdf"
			};

			var dresult = await c.DeleteImage(dpi);
			Assert.True(c.HttpContext.Response.StatusCode == 404);
		}

		[Fact]
		public async Task BuildGetRecent_AfterCreateAndDelete()
		{
			var c = GetController();
			TestHelper.SetupUser(c);
			var bresult = await c.CreateBuild();
			var recent = await c.GetRecent();
			Assert.True(recent.Contains(bresult.Id));
			await c.DeleteBuild(bresult.Id);
			var r2 = await c.GetRecent();
			Assert.True(!r2.Contains(bresult.Id));
		}

		[Fact]
		public async Task BuildSave()
		{
			var c = GetController();
			TestHelper.SetupUser(c);
			var result = await c.CreateBuild();
			result.Title = "test";
			result.Description = "test2";
			await c.Save(result);
			Assert.True(c.HttpContext.Response.StatusCode == 200);
			var b2 = await c.GetById(result.Id);

			Assert.True(c.HttpContext.Response.StatusCode == 200);
			Assert.True(b2.Id == b2.Id);
			Assert.True(b2.Title == b2.Title);
			Assert.True(b2.Description == b2.Description);
		}

		[Fact]
		public async Task BuildSave_NotOwner()
		{
			var c = GetController();
			TestHelper.SetupUser(c);
			var build = await c.CreateBuild();
			TestHelper.SetupUser(c, "u2");
			await c.Save(build);
			Assert.True(c.HttpContext.Response.StatusCode == 403);
		}
	}
}
