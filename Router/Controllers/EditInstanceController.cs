using Microsoft.AspNetCore.Mvc;
using Router.Models;
using System.Diagnostics;

namespace Router.Controllers
{
    public class EditInstanceController : BaseController
    {
        public IActionResult Index()
        {
            if (!CheckSecurity()) { return RedirectToAction("AccessDenied"); }
            var id = Request.Query["id"];
            if(id == "") { return Error(); }
            var instance = App.Config.Charlotte.Instances[App.Config.Charlotte.Instances.FindIndex(a => a.Id == id)];
            return View(instance);
        }

        [HttpPost]
        public IActionResult Update()
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
                App.Config.Charlotte.Instances[App.Config.Charlotte.Instances.FindIndex(a => a.Id == instance.Id)] = instance;
                App.SaveConfig();
            }
            return Redirect("/Dashboard");
        }

        [HttpPost]
        public IActionResult Delete()
        {
            if (!CheckSecurity()) { return RedirectToAction("AccessDenied"); }
            if (Request.HasFormContentType == true)
            {
                var Id = Request.Form["instanceId"];
                var instance = App.Config.Charlotte.Instances[App.Config.Charlotte.Instances.FindIndex(a => a.Id == Id)];
                if(instance != null)
                {
                    App.Config.Charlotte.Instances.Remove(instance);
                    App.SaveConfig();
                }
                else
                {
                    return Error();
                }
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