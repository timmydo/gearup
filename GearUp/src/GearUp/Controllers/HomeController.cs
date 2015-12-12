namespace GearUp.Controllers
{
	using GearUp.Interfaces;
	using Microsoft.AspNet.Mvc;
	using Microsoft.Extensions.Logging;

	public class HomeController : Controller
	{

		public IAppSiteSettings ss { get; set; }
		public HomeController(IAppSiteSettings s, ILogger logger)
		{
			ss = s;
		}

        public IActionResult Index()
        {
			ViewBag.SiteSettings = ss;
            return View();
        }

        public IActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }
    }
}