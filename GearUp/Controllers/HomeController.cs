namespace GearUp.Controllers
{
	using Auth;
	using Microsoft.AspNet.Mvc;
	using Microsoft.Extensions.Logging;
	using Microsoft.Extensions.Options;
	using Models;
	using Shared.Interfaces;
	using System;
	public class HomeController : Controller
	{

		public SiteSettings ss { get; set; }
		private IUserAuthenticator _ua;

		public HomeController(IOptions<SiteSettings> s, ILogger logger, IUserAuthenticator ua)
		{
			ss = s.Value;
			_ua = ua;
		}

        public IActionResult Index()
        {
			ViewBag.SiteSettings = ss;
			var loginInfo = _ua.AuthenticateUser(this);
			if (loginInfo == null)
			{
				ViewBag.UserIsAuthenticated = false;
				ViewBag.UserIdentityName = "";
				ViewBag.UserIdentityKey = "";
			}
			else
			{
				// lets give them a cookie
				if (string.IsNullOrEmpty(loginInfo.Hmac))
				{
					var cookieValue = _ua.GenerateCookie(loginInfo);
					HttpContext.Response.Cookies.Append(UserAuthenticator.CookieName, cookieValue);
				}

				ViewBag.UserIsAuthenticated = true;
				ViewBag.UserIdentityName = loginInfo.FriendlyName;
				ViewBag.UserIdentityKey = loginInfo.UserId;
			}

			return View();
        }

        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }
    }
}