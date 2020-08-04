using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace RaidBot.Controllers
{
    public class HomeController : Controller
    {
        private IHostingEnvironment _env;
        public HomeController(IHostingEnvironment env)
        {
            _env = env;
        }

        /// <summary>
        /// This is a catch-all route that handles any incoming request. It allows the angular application to match any and all routes.
        /// </summary>
        /// <returns></returns>
        [Route("{*url}", Order = int.MaxValue)]
        public IActionResult Index()
        {
            ViewBag.Path = Request.Path;
            ViewBag.Environment = _env;
            return View();
        }
    }
}
