using Microsoft.AspNetCore.Mvc;
using PalmHilsSemanticKernelBot.Models;
using System.Diagnostics;

namespace PalmHilsSemanticKernelBot.Controllers
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

        public IActionResult Chat()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

       
    }
}
