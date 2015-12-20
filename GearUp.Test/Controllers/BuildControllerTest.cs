
namespace GearUp.Test.Controllers
{
	using Auth;
	using GearUp.Controllers;
	using Microsoft.AspNet.Http.Internal;
	using Microsoft.AspNet.Mvc;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Logging;
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

			_serviceProvider = services.BuildServiceProvider();
		}

		public BuildController GetController()
		{
			var bc = new BuildController(_serviceProvider.GetRequiredService<ILogger>(),
				_serviceProvider.GetRequiredService<IPartitionedKeyValueDictionary>());

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
			Assert.True(bobj[Build.CreatorField].ToObject<string>() == uuid);


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
			Assert.True(arr[0][Build.IdField].ToObject<string>().Equals(result));
			Assert.True(arr[1][Build.IdField].ToObject<string>().Equals(result2));
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
		public async Task BuildDeleteImage()
		{
			var c = GetController();
			setupUser(c);
			var result = await c.CreateBuild();
			
			Assert.False(true);
		}

		[Fact]
		public async Task BuildDeleteImage_NotOwner()
		{
			var c = GetController();
			var result = await c.CreateBuild();
			Assert.False(true);
		}

		[Fact]
		public async Task BuildDeleteImage_BadImage()
		{
			var c = GetController();
			var result = await c.CreateBuild();
			Assert.False(true);
		}

		[Fact]
		public async Task BuildDeleteImage_EmptyParams()
		{
			var c = GetController();
			var result = await c.CreateBuild();
			Assert.False(true);
		}

		[Fact]
		public async Task BuildGetRecent_AfterCreate()
		{
			var c = GetController();
			var result = await c.CreateBuild();
			Assert.False(true);
		}

		[Fact]
		public async Task BuildSave()
		{
			var c = GetController();
			var result = await c.CreateBuild();
			Assert.False(true);
		}

		[Fact]
		public async Task BuildSave_NotOwner()
		{
			var c = GetController();
			var result = await c.CreateBuild();
			Assert.False(true);
		}

		[Fact]
		public async Task BuildSave_NotLLoggedIn()
		{
			var c = GetController();
			var result = await c.CreateBuild();
			Assert.False(true);
		}


	}
}
