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


	public class ListControllerTest
    {
		private readonly IServiceProvider _serviceProvider;

		public ListControllerTest()
		{
			var services = new ServiceCollection();
			var lf = new LoggerFactory();
			services.AddScoped<IPartitionedKeyValueDictionary>((sp) => new MockDictionary());
			services.AddScoped<ILogger>((sp) => lf.CreateLogger(string.Empty));

			_serviceProvider = services.BuildServiceProvider();
		}

		private ListController GetController()
		{
			var c = new ListController(
				_serviceProvider.GetRequiredService<IPartitionedKeyValueDictionary>(),
				_serviceProvider.GetRequiredService<ILogger>()
				);

			c.ControllerContext.HttpContext = new DefaultHttpContext();
			return c;
		}


		[Fact]
		public async Task ListAddBuild_ListNotFound()
		{
			var c = GetController();
			var result = await c.GetById("123");
			Assert.False(true);
		}

		[Fact]
		public async Task ListAddBuild_BuildNotFound()
		{
			var c = GetController();
			var result = await c.GetById("123");
			Assert.False(true);
		}

		[Fact]
		public async Task ListAddBuild_NotUsersList()
		{
			var c = GetController();
			var result = await c.GetById("123");
			Assert.False(true);
		}

		[Fact]
		public async Task ListAddBuild_Success()
		{
			var c = GetController();
			var result = await c.GetById("123");
			Assert.False(true);
		}

		[Fact]
		public async Task ListRemoveBuild_Success()
		{
			var c = GetController();
			var result = await c.GetById("123");
			Assert.False(true);
		}

		[Fact]
		public async Task ListRemoveBuild_ListNotFound()
		{
			var c = GetController();
			var result = await c.GetById("123");
			Assert.False(true);
		}

		[Fact]
		public async Task ListRemoveBuild_BuildNotFound()
		{
			var c = GetController();
			var result = await c.GetById("123");
			Assert.False(true);
		}

		[Fact]
		public async Task ListRemoveBuild_NotUsersList()
		{
			var c = GetController();
			var result = await c.GetById("123");
			Assert.False(true);
		}

		[Fact]
		public async Task ListDelete_Success()
		{
			var c = GetController();
			var result = await c.GetById("123");
			Assert.False(true);
		}

		[Fact]
		public async Task ListDelete_NotOwner()
		{
			var c = GetController();
			var result = await c.GetById("123");
			Assert.False(true);
		}

		[Fact]
		public async Task ListDelete_NotFound()
		{
			var c = GetController();
			var result = await c.GetById("123");
			Assert.False(true);
		}

		[Fact]
		public async Task ListSave_Success()
		{
			var c = GetController();
			var result = await c.GetById("123");
			Assert.False(true);
		}

		[Fact]
		public async Task ListSave_NotOwner()
		{
			var c = GetController();
			var result = await c.GetById("123");
			Assert.False(true);
		}

		[Fact]
		public async Task ListSave_ListNotFound()
		{
			var c = GetController();
			var result = await c.GetById("123");
			Assert.False(true);
		}


	}
}
