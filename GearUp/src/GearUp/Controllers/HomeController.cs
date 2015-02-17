using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc;
using GearUp.Services;
using Microsoft.Framework.Logging;

namespace GearUp.Controllers
{
    public class HomeController : Controller
    {

		public SiteSettings ss { get; set; }
		public HomeController(SiteSettings s, ILogger logger)
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