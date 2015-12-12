namespace GearUp.Controllers
{
	using Microsoft.AspNet.Mvc;
	using GearUp.Models;
	using System.Threading.Tasks;
	using GearUp.Services;
	using Microsoft.Extensions.Logging;

	[Route("api/[controller]")]
	public class ListController : Controller
    {

		private readonly ILogger _logger;
		private readonly DataService _ddb;

		public ListController(DataService ddb, ILogger logger)
		{
			this._ddb = ddb;
			this._logger = logger;
		}


		[Produces("application/json", "text/json")]
		[HttpGet("{id}")]
		public async Task<BuildList> GetById(string id)
		{
			var b = await this._ddb.GetListAsync(id);
			if (b == null)
			{
				b = new BuildList();
				b.id = id;
				var uid = UserLogin.UserUniqueId(User.Identity);
				b.Creator = uid;
				if (string.IsNullOrEmpty(uid))
				{
					b.Title = "Must log in to create lists";
					return b;
				}
				this._logger.LogInformation("Setting creator to " + uid);
				await this._ddb.CreateListAsync(b);
			}
			return b;
		}


	}
}
