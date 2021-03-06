﻿namespace GearUp.Test.Controllers
{
	using GearUp.Controllers;
	using GearUp.Services;
	using Microsoft.AspNet.Http.Internal;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Logging;
	using Shared.Interfaces;
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Xunit;


	public class ListControllerTest
    {
		private readonly IServiceProvider _serviceProvider;

		public ListControllerTest()
		{
			var services = new ServiceCollection();
			var lf = new LoggerFactory();
			services.AddScoped<IPartitionedKeyValueDictionary>((sp) => new LocalDictionary());
			services.AddScoped<ILogger>((sp) => lf.CreateLogger(string.Empty));

			_serviceProvider = services.BuildServiceProvider();
		}

		private ListController GetController()
		{
			var c = new ListController(
				_serviceProvider.GetRequiredService<IPartitionedKeyValueDictionary>(),
				_serviceProvider.GetRequiredService<ILogger>(),
				_serviceProvider.GetRequiredService<IUserAuthenticator>()
				);

			c.ControllerContext.HttpContext = new DefaultHttpContext();
			return c;
		}

		[Fact]
		public async Task ListCreate_NotLoggedIn()
		{
			var c = GetController();
			var l = await c.CreateList();
			Assert.True(c.HttpContext.Response.StatusCode == 401);
		}

		[Fact]
		public async Task ListCreateGet_Success()
		{
			var c = GetController();
			TestHelper.SetupUser(c);
			var l = await c.CreateList();
			Assert.True(c.HttpContext.Response.StatusCode == 200);
			var result = await c.GetById(l.Id);
			Assert.True(c.HttpContext.Response.StatusCode == 200);
			Assert.True(result.Id == l.Id);
		}

		[Fact]
		public async Task List_GetMultiple()
		{
			var c = GetController();
			TestHelper.SetupUser(c);
			var result = await c.CreateList();
			Assert.True(c.HttpContext.Response.StatusCode == 200);

			var result2 = await c.CreateList();
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
		public async Task ListGet_NotFound()
		{
			var c = GetController();
			var result = await c.GetById("123");
			Assert.True(c.HttpContext.Response.StatusCode == 404);
			Assert.True(result == null);
		}

		[Fact]
		public async Task ListDelete_Success()
		{
			var c = GetController();
			TestHelper.SetupUser(c);
			var l = await c.CreateList();
			var result = await c.DeleteList(l.Id);
			Assert.True(c.HttpContext.Response.StatusCode == 200);
		}

		[Fact]
		public async Task ListDelete_NotOwner()
		{
			var c = GetController();
			TestHelper.SetupUser(c, "1");
			var l = await c.CreateList();
			TestHelper.SetupUser(c, "2");
			var result = await c.DeleteList(l.Id);
			Assert.True(c.HttpContext.Response.StatusCode == 403);
		}

		[Fact]
		public async Task ListDelete_NotFound()
		{
			var c = GetController();
			var result = await c.DeleteList("abc");
			Assert.True(c.HttpContext.Response.StatusCode == 404);
		}

		[Fact]
		public async Task ListSave_Null()
		{
			var c = GetController();
			TestHelper.SetupUser(c);
			await c.Save(null);
			Assert.True(c.HttpContext.Response.StatusCode == 400);
		}

		[Fact]
		public async Task ListSave_Success()
		{
			var c = GetController();
			TestHelper.SetupUser(c);
			var l = await c.CreateList();
			l.Title = "1";
			l.Description = "2";
			await c.Save(l);
			Assert.True(c.HttpContext.Response.StatusCode == 200);
			var l2 = await c.GetById(l.Id);
			Assert.True(c.HttpContext.Response.StatusCode == 200);
			Assert.True(l2.Id == l.Id);
			Assert.True(l2.Title == l.Title);
			Assert.True(l2.Description == l.Description);
		}

		[Fact]
		public async Task ListSave_NotOwner()
		{
			var c = GetController();
			TestHelper.SetupUser(c, "1");
			var l = await c.CreateList();
			TestHelper.SetupUser(c, "2");
			await c.Save(l);
			Assert.True(c.HttpContext.Response.StatusCode == 403);
		}

		[Fact]
		public async Task ListSave_ListNotFound()
		{
			var c = GetController();
			TestHelper.SetupUser(c);
			var l = await c.CreateList();
			l.Id = "a";
			await c.Save(l);
			Assert.True(c.HttpContext.Response.StatusCode == 404);
		}
	}
}
