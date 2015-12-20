
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
	using Xunit;

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
			Assert.True(string.IsNullOrEmpty(result));
		}

		[Fact]
		public async Task CreateBuild_MustLogIn()
		{
			var c = GetController();
			c.HttpContext.User = new ClaimsPrincipal();
			var result = await c.CreateBuild();

			Assert.True(c.HttpContext.Response.StatusCode == 401);
			Assert.True(string.IsNullOrEmpty(result));
		}

		[Fact]
		public async Task CreateBuild_GetAndDeleteBuild()
		{
			var c = GetController();

			setupUser(c);
			
			var uuid = UserLogin.UserUniqueId(c.HttpContext.User.Identity);
			var result = await c.CreateBuild();

			Assert.True(c.HttpContext.Response.StatusCode == 200);
			Assert.False(string.IsNullOrEmpty(result));

			var result2 = await c.GetById(result);

			Assert.True(c.HttpContext.Response.StatusCode == 200);
			Assert.False(string.IsNullOrEmpty(result2));

			var bobj = JObject.Parse(result2);
			Assert.True(bobj[Build.CreatorFieldName].ToObject<string>() == uuid);


			await c.DeleteBuild(result);
			Assert.True(c.HttpContext.Response.StatusCode == 200);

			var result3 = await c.GetById(result);

			Assert.True(c.HttpContext.Response.StatusCode == 404);
			Assert.True(string.IsNullOrEmpty(result3));
		}

		private void setupUser(Controller c, string name = "user")
		{
			var claims = new List<Claim> {
				new Claim(ClaimTypes.NameIdentifier, name),
				new Claim("urn:google:something", "asdf")};

			c.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
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
			var result = await c.CreateBuild();
			await c.DeleteBuild(result);
			Assert.True(c.HttpContext.Response.StatusCode == 404);
		}

		[Fact]
		public async Task Build_GetMultiple()
		{
			var c = GetController();

			var result = await c.CreateBuild();
			Assert.True(c.HttpContext.Response.StatusCode == 200);

			var result2 = await c.CreateBuild();
			Assert.True(c.HttpContext.Response.StatusCode == 200);

			var bl = new BuildList();
			bl.Builds = new List<string>()
			{
				result,
				result2
			};

			var result3 = await c.GetMultiple(bl);
			var arr = JArray.Parse(result3);
			Assert.True(c.HttpContext.Response.StatusCode == 200);
			Assert.True(arr[0][Build.IdFieldName].ToObject<string>().Equals(result));
			Assert.True(arr[1][Build.IdFieldName].ToObject<string>().Equals(result2));
		}

		[Fact]
		public async Task Build_GetMultiple_EmptyList()
		{
			var c = GetController();

			var bl = new BuildList();
			bl.Builds = new List<string>()
			{
			};

			var result3 = await c.GetMultiple(bl);
			var arr = JArray.Parse(result3);
			Assert.True(c.HttpContext.Response.StatusCode == 200);
			Assert.True(arr.Count == 0);
		}

		[Fact]
		public async Task Build_GetMultiple_NotExist()
		{
			var c = GetController();

			var bl = new BuildList();
			bl.Builds = new List<string>()
			{
				"abc"
			};

			var result3 = await c.GetMultiple(bl);
			var arr = JArray.Parse(result3);
			Assert.True(c.HttpContext.Response.StatusCode == 200);
			Assert.True(arr.Count == 0);
		}

		[Fact]
		public async Task BuildAddImage_DeleteImage_Success()
		{
			var c = GetController();
			setupUser(c);
			var bresult = await c.CreateBuild();
			Assert.True(c.HttpContext.Response.StatusCode == 200);

			c.Request.HttpContext.Request.ContentType = c.ValidContentTypes[0];
			var iresult = await c.AddImage(bresult);
			Assert.True(c.HttpContext.Response.StatusCode == 200);

			// check to see if the image exists
			var newb = await c.GetById(bresult);
			var bobj = JObject.Parse(newb);
			var images = bobj[Build.ImageFieldName] as JArray;
			Assert.True(images[0][Image.GuidFieldName].ToObject<string>() == iresult);

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
			setupUser(c);
			c.Request.HttpContext.Request.ContentType = c.ValidContentTypes[0];
			var result = await c.AddImage("abc");

			Assert.True(c.HttpContext.Response.StatusCode == 404);
		}

		[Fact]
		public async Task BuildAddImage_NotOwner()
		{
			var c = GetController();
			setupUser(c);
			var bresult = await c.CreateBuild();
			c.Request.HttpContext.Request.ContentType = c.ValidContentTypes[0];
			setupUser(c, "asdf");
			var iresult = await c.AddImage(bresult);

			Assert.True(c.HttpContext.Response.StatusCode == 403);
		}

		[Fact]
		public async Task BuildAddImage_BadContentType()
		{
			var c = GetController();
			setupUser(c);
			var bresult = await c.CreateBuild();
			c.Request.HttpContext.Request.ContentType = "asdf";
			var result = await c.AddImage(bresult);
			Assert.True(c.HttpContext.Response.StatusCode == 400);
		}

		[Fact]
		public async Task BuildDeleteImage_NotOwner()
		{
			var c = GetController();
			setupUser(c);
			var bresult = await c.CreateBuild();
			Assert.True(c.HttpContext.Response.StatusCode == 200);

			c.Request.HttpContext.Request.ContentType = c.ValidContentTypes[0];
			var iresult = await c.AddImage(bresult);
			Assert.True(c.HttpContext.Response.StatusCode == 200);

			var dpi = new BuildController.DeleteImageParamInfo
			{
				Build = bresult,
				Image = iresult
			};
			setupUser(c, "abc");
			var dresult = await c.DeleteImage(dpi);
			Assert.True(c.HttpContext.Response.StatusCode == 403);
		}

		[Fact]
		public async Task BuildDeleteImage_BadImage()
		{
			var c = GetController();
			var result = await c.CreateBuild();
			Assert.True(false);
		}

		[Fact]
		public async Task BuildDeleteImage_BadBuild()
		{
			var c = GetController();
			var result = await c.CreateBuild();
			Assert.True(false);
		}

		[Fact]
		public async Task BuildGetRecent_AfterCreate()
		{
			var c = GetController();
			var result = await c.CreateBuild();
			Assert.True(false);
		}

		[Fact]
		public async Task BuildSave()
		{
			var c = GetController();
			var result = await c.CreateBuild();
			Assert.True(false);
		}

		[Fact]
		public async Task BuildSave_NotOwner()
		{
			var c = GetController();
			var result = await c.CreateBuild();
			Assert.True(false);
		}

		[Fact]
		public async Task BuildSave_NotLLoggedIn()
		{
			var c = GetController();
			var result = await c.CreateBuild();
			Assert.True(false);
		}


	}
}
