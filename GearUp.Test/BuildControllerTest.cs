
namespace GearUp.Test
{
	using GearUp.Controllers;
	using Microsoft.AspNet.Http.Internal;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Logging;
	using Shared.Interfaces;
	using System;
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

		private BuildController GetController()
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
		public async Task CreateBuild_GetAndDeleteBuild()
		{
			var c = GetController();

			var result = await c.CreateBuild();

			Assert.True(c.HttpContext.Response.StatusCode == 200);
			Assert.False(string.IsNullOrEmpty(result));

			var result2 = await c.GetById(result);

			Assert.True(c.HttpContext.Response.StatusCode == 200);
			Assert.False(string.IsNullOrEmpty(result2));

			await c.DeleteBuild(result);
			Assert.True(c.HttpContext.Response.StatusCode == 200);

			var result3 = await c.GetById(result);

			Assert.True(c.HttpContext.Response.StatusCode == 404);
			Assert.True(string.IsNullOrEmpty(result3));
		}

		[Fact]
		public async Task DeleteBuild_NotFound()
		{
			var c = GetController();
		}

		[Fact]
		public async Task DeleteBuild_NotOwner()
		{
			var c = GetController();
		}
	}
}
