using System.Collections.Generic;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace GearUp.Controllers.Controllers
{
	[Route("api/login")]
    public class LoginController : Controller
    {

		class LoginInfo
		{
			public string username { get; set; }
			public string password { get; set; }
			public string remember { get; set; }
		}


        // GET: api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public JsonResult Post([FromBody]string value)
        {
			var li = JsonConvert.DeserializeObject<LoginInfo>(value);

			return Json(new { authResult = "abc123"});
        }


    }
}
