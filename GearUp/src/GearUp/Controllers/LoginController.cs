using System.Collections.Generic;
using Microsoft.AspNet.Mvc;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace GearUp.Controllers.Controllers
{
	[Route("api/[controller]")]
    public class LoginController : Controller
    {

		public class LoginInfo
		{
			public string username { get; set; }
			public string password { get; set; }
			public string remember { get; set; }
		}

		

        // POST api/values
  //      [HttpPost]
  //      public JsonResult Post([FromBody]LoginInfo value)
  //      {
		//	if (value == null)
		//	{
		//		return Json(new { error = "login post value was empty"});
		//	}
		//	else
		//	{
		//		return Json(new { authResult = value.username + "_abc123" });
		//	}
		//}


    }
}
