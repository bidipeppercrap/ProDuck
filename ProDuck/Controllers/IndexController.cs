using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ProDuck.Controllers
{
    [Route("/")]
    [ApiController]
    public class IndexController : ControllerBase
    {
        [HttpGet] public ActionResult<string> Index()
        {
            return "Welcome to ProDuck - bidipeppercrap";
        }
    }
}
