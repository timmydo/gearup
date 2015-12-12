namespace GearUp.Controllers
{
	using Microsoft.AspNet.Mvc;

	[Route("api/[controller]")]
	public class HelloController : Controller
    {
		public HelloController()
		{
		}


		[HttpGet()]
		public string Get()
		{
			return "Hello 123";
		}

	}
}
