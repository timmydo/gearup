namespace GearUp.Controllers
{
	using Microsoft.AspNet.Mvc;
	using Microsoft.Extensions.Logging;
	using Microsoft.Extensions.OptionsModel;
	using Models;
	public class HomeController : Controller
	{

		public SiteSettings ss { get; set; }
		public HomeController(IOptions<SiteSettings> s, ILogger logger)
		{
			ss = s.Value;
		}

        public IActionResult Index()
        {
			ViewBag.SiteSettings = ss;
            return View();
        }

        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }
    }
}