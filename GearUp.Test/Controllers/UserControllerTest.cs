namespace GearUp.Test.Controllers
{
	using Auth;
	using GearUp.Controllers;
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

	public class UserControllerTest
    {
		private readonly IServiceProvider _serviceProvider;

		public UserControllerTest()
		{
			var services = new ServiceCollection();
			var lf = new LoggerFactory();
			services.AddScoped<IPartitionedKeyValueDictionary>((sp) => new MockDictionary());
			services.AddScoped<ILogger>((sp) => lf.CreateLogger(string.Empty));

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
		public async Task ListGetUserBuilds_NotLoggedIn()
		{
			var c = GetController();
			var result = await c.UserBuilds("123");
			Assert.False(true);
		}

		[Fact]
		public async Task ListGetUserBuilds_Success()
		{
			var c = GetController();
			var result = await c.UserBuilds("123");
			Assert.False(true);
		}


		[Fact]
		public async Task ListGetUserLists_Success()
		{
			var c = GetController();
			var result = await c.UserBuilds("123");
			Assert.False(true);
		}

	}
}
