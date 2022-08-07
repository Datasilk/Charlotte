using Microsoft.AspNetCore.Mvc;

namespace Charlotte.Controllers
{
    public class BrowserController : ControllerBase
    {
        [HttpPost]
        public string Index(string url, string macros = "")
        {
            //create new browser instance and collect DOM from web page in JSON format
            return Chrome.Browser.Collect(url);
        }
    }
}