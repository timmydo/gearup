namespace GearUp.Test.Controllers
{
	using Microsoft.AspNet.Mvc;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Claims;
	using System.Threading.Tasks;

	public class TestHelper
    {
		public static void SetupUser(Controller c, string name = "user")
		{
			var claims = new List<Claim> {
				new Claim(ClaimTypes.NameIdentifier, name),
				new Claim("urn:google:something", "asdf")};

			c.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
		}
	}
}
