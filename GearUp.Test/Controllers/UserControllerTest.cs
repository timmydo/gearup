namespace GearUp.Test.Controllers
{
	using Auth;
	using GearUp.Controllers;
	using GearUp.Services;
	using Interfaces;
	using Microsoft.AspNet.Http.Internal;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Logging;
	using Mocks;
	using Shared.Interfaces;
	using System;
	using System.Threading.Tasks;
	using Xunit;

	public class UserControllerTest
    {
		private readonly IServiceProvider _serviceProvider;

		public UserControllerTest()
		{
			var services = new ServiceCollection();
			var lf = new LoggerFactory();
			services.AddScoped<IPartitionedKeyValueDictionary>((sp) => new LocalDictionary());
			services.AddScoped<ILogger>((sp) => lf.CreateLogger(string.Empty));
			services.AddScoped<IAppBlobStorage>((sp) => new MockBlobStorage());

			_serviceProvider = services.BuildServiceProvider();
		}

		private UserController GetController()
		{
			var c = new UserController(
				_serviceProvider.GetRequiredService<IPartitionedKeyValueDictionary>(),
				_serviceProvider.GetRequiredService<ILogger>()
				);

			c.ControllerContext.HttpContext = new DefaultHttpContext();
			return c;
		}

		[Fact]
		public async Task UserGetBuilds_Success()
		{
			var uc = GetController();
			var bc = new BuildController(_serviceProvider.GetRequiredService<ILogger>(),
				_serviceProvider.GetRequiredService<IPartitionedKeyValueDictionary>(),
				_serviceProvider.GetRequiredService<IAppBlobStorage>()
				);
			bc.ControllerContext.HttpContext = new DefaultHttpContext();

			TestHelper.SetupUser(uc);
			TestHelper.SetupUser(bc);
			var build = await bc.CreateBuild();
			Assert.True(bc.HttpContext.Response.StatusCode == 200);

			var user = UserLogin.UserUniqueId(uc.User?.Identity);
			var bl = await uc.UserBuilds(user);
			Assert.True(uc.HttpContext.Response.StatusCode == 200);
			Assert.True(bl.Contains(build.Id));

			await bc.DeleteBuild(build.Id);
			Assert.True(uc.HttpContext.Response.StatusCode == 200);
			var bl2 = await uc.UserBuilds(user);
			Assert.True(uc.HttpContext.Response.StatusCode == 200);
			Assert.True(!bl2.Contains(build.Id));
		}


		[Fact]
		public async Task UserGetLists_Success()
		{
			var uc = GetController();
			var lc = new ListController(_serviceProvider.GetRequiredService<IPartitionedKeyValueDictionary>(),
				_serviceProvider.GetRequiredService<ILogger>()
				);
			lc.ControllerContext.HttpContext = new DefaultHttpContext();

			TestHelper.SetupUser(uc);
			TestHelper.SetupUser(lc);
			var list = await lc.CreateList();
			Assert.True(lc.HttpContext.Response.StatusCode == 200);

			var user = UserLogin.UserUniqueId(uc.User?.Identity);
			var ul = await uc.UserLists(user);
			Assert.True(uc.HttpContext.Response.StatusCode == 200);
			Assert.True(ul.Contains(list.Id));

			await lc.DeleteList(list.Id);
			Assert.True(uc.HttpContext.Response.StatusCode == 200);
			var ul2 = await uc.UserLists(user);
			Assert.True(uc.HttpContext.Response.StatusCode == 200);
			Assert.True(!ul2.Contains(list.Id));
		}

	}
}
