using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebOptimizer.Core.Sample2.Models;

namespace WebOptimizer.Core.Sample2.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }


        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
