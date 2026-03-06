using Microsoft.AspNetCore.Mvc;
using RiviuFood.Web.Models;
using System.Diagnostics;

namespace RiviuFood.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
