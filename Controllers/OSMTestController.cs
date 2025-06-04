using Microsoft.AspNetCore.Mvc;

namespace OSMTest.Controllers
{
    [Route("[controller]")]
    public class OSMTestController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
