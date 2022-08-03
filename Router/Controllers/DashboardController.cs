using Microsoft.AspNetCore.Mvc;
using Router.Models;
using System.Diagnostics;

namespace Router.Controllers
{
    public class DashboardController : BaseController
    {
        public IActionResult Index()
        {
            if (!CheckSecurity()) { return RedirectToAction("AccessDenied"); }

            return View(new DashboardViewModel() { Config = App.Config });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}