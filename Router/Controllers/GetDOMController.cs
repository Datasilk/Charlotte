using System;
using System.Text;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Router.Models;
using System.Diagnostics;
using Router.Common;

namespace Router.Controllers
{
    public class GetDOMController : BaseController
    {
        [HttpPost]
        public string Index(string url, bool session = false, string macros = "")
        {
            var requestId = 0;
            return Charlotte.GetDOM(url, session, macros, out requestId);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}