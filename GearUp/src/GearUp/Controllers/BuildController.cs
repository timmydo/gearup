

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

using Microsoft.AspNet.Mvc;
using Microsoft.Framework.Logging;
using GearUp.Models;
using System.Threading.Tasks;

namespace GearUp.Controllers
{
	[Route("api/[controller]")]
	public class BuildController : Controller
    {

		private readonly ILogger _logger;

		public BuildController(SiteSettings settings, ILogger logger)
		{
			this._logger = logger;
		}


		[Produces("application/json", "text/json")]
		[HttpGet("{id}")]
		public async Task<Build> GetById(string id)
		{
			await Task.Yield();
			var b = new Build() { Id = id };
			return b;
		}


	}
}
