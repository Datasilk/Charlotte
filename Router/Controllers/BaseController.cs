using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Router.Controllers
{
    public class BaseController : Controller
    {
        public bool CheckSecurity()
        {
            return HttpContext.Session.Keys.Contains("admin") && HttpContext.Session.GetString("admin") == "1";
        }
    }
}