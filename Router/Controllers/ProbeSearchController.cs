using System;
using Microsoft.AspNetCore.Mvc;
using Router.Common;

namespace Router.Controllers
{
    /// <summary>
    /// Forward a JS-driven search probe to a Charlotte instance — the caller
    /// gets back the request template the site's search actually fired, so it
    /// can be replayed as a discovered search engine.
    /// </summary>
    public class ProbeSearchController : BaseController
    {
        [HttpPost]
        public string Index(string url, string probe = "")
        {
            return Charlotte.ProbeSearch(url, probe);
        }
    }
}
