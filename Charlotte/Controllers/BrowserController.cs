using Microsoft.AspNetCore.Mvc;

namespace Charlotte.Controllers
{
    public class BrowserController : ControllerBase
    {
        [HttpPost]
        public string Index(string url, string macros = "", string backend = "")
        {
            // ?backend=cefsharp|playwright pins one backend (A/B testing);
            // anything else rides the composite primary+fallback chain.
            if (!string.IsNullOrWhiteSpace(backend)
                && Chrome.Backends.TryGetValue(backend, out var pinned))
                return pinned.Collect(url);
            //create new browser instance and collect DOM from web page in JSON format
            return Chrome.Browser.Collect(url);
        }

        /// <summary>
        /// Drive the page's JS search with a probe term and return the request
        /// the site actually fired (url/method/postData/headers as JSON).
        /// Playwright-only — CefSharp can't tap the network stack the same way.
        /// </summary>
        [HttpPost]
        public string ProbeSearch(string url, string probe = "")
        {
            if (Chrome.Backends.TryGetValue("playwright", out var b) && b is PlaywrightBrowser pw)
                return pw.ProbeSearch(url, probe);
            return "{\"found\":false,\"error\":\"playwright backend unavailable\"}";
        }
    }
}