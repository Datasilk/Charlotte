using Microsoft.AspNetCore.Mvc;
using Router.Models;
using System.Diagnostics;
using System.Text;

namespace Router.Controllers
{
    public class HomeController : BaseController
    {
        public IActionResult Index()
        {
            if(Request.HasFormContentType)
            {
                //check for login
                var username = Request.Form["username"];
                var password = Request.Form["password"];
                if (username != "" && password != "")
                {
                    if(username == App.Config.Admin.Username && password == App.Config.Admin.Pass)
                    {
                        HttpContext.Session.SetString("admin", "1");
                        Response.Redirect("Dashboard");
                    }
                }
            }
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}