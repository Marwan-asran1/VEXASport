using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VEXA.Models;

namespace VEXA.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Categories()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Profile()
        {
            return View();
        }

        public IActionResult Cart()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Men()
        {
            return View();
        }
        
        public IActionResult Women()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
