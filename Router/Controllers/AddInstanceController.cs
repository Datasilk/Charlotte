using Microsoft.AspNetCore.Mvc;
using Router.Models;
using System.Diagnostics;

namespace Router.Controllers
{
    public class AddInstanceController : BaseController
    {
        public IActionResult Index()
        {
            if (!CheckSecurity()) { return RedirectToAction("AccessDenied"); }
            return View(new { AddingInstance = true});
        }

        [HttpPost]
        public IActionResult Create()
        {
            if (!CheckSecurity()) { return RedirectToAction("AccessDenied"); }
            if(Request.HasFormContentType == true)
            {
                var instance = new ConfigCharlotteInstance();

                instance.Id = Request.Form["instanceId"];
                instance.Url = Request.Form["url"];
                instance.ServerName = Request.Form["serverName"];
                instance.Note = Request.Form["note"];
                instance.UsesCookies = Request.Form["usesCookies"] == "1";
                if(App.Config.Charlotte == null)
                {
                    App.Config.Charlotte = new ConfigCharlotte();
                    App.Config.Charlotte.Instances = new List<ConfigCharlotteInstance>();
                }
                App.Config.Charlotte.Instances.Add(instance);
                App.SaveConfig();
            }
            return Redirect("/Dashboard");
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