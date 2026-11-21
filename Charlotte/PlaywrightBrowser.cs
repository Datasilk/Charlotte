using System.Text;
using System.Text.Json;
using Microsoft.Playwright;

namespace Charlotte
{
    /// <summary>
    /// Playwright headless-Chromium backend — an alternative to the CefSharp
    /// <see cref="Browser"/> behind the same <see cref="IBrowser"/> contract.
    /// Runs the identical check-lazyload / lazyload / extractDOM scripts so the
    /// JSON returned to Html.DeserializeArticle is byte-for-byte the same shape.
    /// </summary>
    public class PlaywrightBrowser : IBrowser
    {
        private IPlaywright _playwright = null!;
        private IBrowserContext _context = null!;
        private string checklazyjs = "";
        private string lazyjs = "";
        private string extractDOMjs = "";

        private const string DesktopUserAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/139.0.0.0 Safari/537.36";

        public static PlaywrightBrowser Create()
        {
            var b = new PlaywrightBrowser();
            b.Initialize().GetAwaiter().GetResult();
            return b;
        }

        private async Task Initialize()
        {
            Console.WriteLine("Initialize Playwright Browser");

            // Persistent profile dir — cookies/storage survive runs like
            // CefSharp's CachePath (a session-less visitor is a bot signal).
            var dataDir = App.MapPath("playwright-profile");
            Directory.CreateDirectory(dataDir);

            _playwright = await Playwright.CreateAsync();
            _context = await _playwright.Chromium.LaunchPersistentContextAsync(dataDir,
                new BrowserTypeLaunchPersistentContextOptions
                {
                    Headless = true,
                    UserAgent = DesktopUserAgent,
                    ViewportSize = new ViewportSize { Width = 1366, Height = 900 },
                    Args = new[]
                    {
                        "--disable-blink-features=AutomationControlled",
                        "--disable-gpu", "--disable-dev-shm-usage"
                    }
                });

            // Match CefSharp's disable-image-loading — drop image/font/media
            // requests at the router (faster, and images aren't needed for DOM).
            await _context.RouteAsync("**/*", async route =>
            {
                var t = route.Request.ResourceType;
                if (t == "image" || t == "media" || t == "font")
                    await route.AbortAsync();
                else
                    await route.ContinueAsync();
            });

            var baseDir = App.RootPath + "/";
            checklazyjs = ReadScript(baseDir, "check-lazyload.js");
            lazyjs = ReadScript(baseDir, "lazyload.js");
            extractDOMjs = ReadScript(baseDir, "extractDOM.js");
        }

        private static string ReadScript(string baseDir, string name)
        {
            var path = baseDir + name;
            if (!File.Exists(path))
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, name);
            return File.ReadAllText(path);
        }

        public string Collect(string url)
        {
            try
            {
                return CollectAsync(url).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                return "error: " + ex.Message + "\n\n" + ex.StackTrace;
            }
        }

        private async Task<string> CollectAsync(string url)
        {
            var log = new StringBuilder();
            log.AppendLine("Opening Web Page: " + url);
            var page = await _context.NewPageAsync();
            try
            {
                // 20s nav budget (matches the CefSharp backend's wait loop).
                // DOMContentLoaded rather than Load — analytics/trackers hold
                // 'load' open on news sites long past useful content.
                try
                {
                    await page.GotoAsync(url, new PageGotoOptions
                    {
                        WaitUntil = WaitUntilState.DOMContentLoaded,
                        Timeout = 20000
                    });
                }
                catch (TimeoutException)
                {
                    log.AppendLine("Navigation hit 20s budget — extracting what loaded");
                }
                log.AppendLine("End loading main frame: " + page.Url);

                // Same lazy-content dance as the CefSharp backend.
                var hasLazy = await page.EvaluateAsync<bool>(
                    "() => { " + checklazyjs + " }");
                if (hasLazy)
                {
                    await page.EvaluateAsync(lazyjs);
                    await Task.Delay(2000);
                }

                // Serialize in-page: Playwright's wire protocol caps return
                // values at depth 64 and real DOMs nest far deeper. The
                // stringified result is already the exact DOM-JSON contract.
                var html = await page.EvaluateAsync<string>(
                    "() => JSON.stringify((() => { " + extractDOMjs + " })())");
                if (string.IsNullOrWhiteSpace(html) || html == "null")
                    return "error: No HTML collected via JavaScript Injection into web page";
                return html;
            }
            finally
            {
                try { await page.CloseAsync(); } catch { }
            }
        }

        /// <summary>
        /// Drive the page's JS-driven search with a probe term and capture
        /// the request the site actually fires — the observed endpoint is the
        /// search engine template we can replay confidently. Returns JSON:
        /// {"found":true,"request":{url,method,resourceType,postData,headers}}
        /// or {"found":false,"error":"..."}.
        /// </summary>
        public string ProbeSearch(string url, string probe)
        {
            try { return ProbeSearchAsync(url, probe).GetAwaiter().GetResult(); }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { found = false, error = ex.Message });
            }
        }

        private class CapturedRequest
        {
            public string Url = "";
            public string Method = "";
            public string ResourceType = "";
            public string? PostData;
            public Dictionary<string, string>? Headers;
            public string? ResponseContentType;
        }

        private static readonly string[] SearchInputSelectors =
        {
            "input[type=search]",
            "input[name=\"q\" i]", "input[name=\"s\" i]", "input[name=\"query\" i]",
            "input[name=\"search\" i]", "input[name=\"keyword\" i]",
            "input[placeholder*=\"search\" i]", "input[aria-label*=\"search\" i]",
            "input[id*=\"search\" i]", "input[name*=\"search\" i]"
        };

        private async Task<string> ProbeSearchAsync(string url, string probe)
        {
            if (string.IsNullOrWhiteSpace(probe)) probe = "collectorprobe42";
            var page = await _context.NewPageAsync();
            var captured = new List<CapturedRequest>();
            page.Request += (_, req) =>
            {
                try
                {
                    lock (captured)
                    {
                        captured.Add(new CapturedRequest
                        {
                            Url = req.Url,
                            Method = req.Method,
                            ResourceType = req.ResourceType,
                            PostData = req.PostData,
                            Headers = req.Headers
                        });
                    }
                }
                catch { /* request capture is best-effort */ }
            };
            page.Response += (_, resp) =>
            {
                try
                {
                    var ct = resp.Headers != null && resp.Headers.TryGetValue("content-type", out var v) ? v : null;
                    if (ct == null) return;
                    lock (captured)
                    {
                        var match = captured.FirstOrDefault(r => r.Url == resp.Url && r.Method == resp.Request.Method);
                        if (match != null) match.ResponseContentType = ct;
                    }
                }
                catch { /* response capture is best-effort */ }
            };

            try
            {
                try
                {
                    await page.GotoAsync(url, new PageGotoOptions
                    {
                        WaitUntil = WaitUntilState.DOMContentLoaded,
                        Timeout = 20000
                    });
                }
                catch (TimeoutException) { /* use whatever loaded */ }

                // Let JS widgets mount before looking for the input.
                await page.WaitForTimeoutAsync(1500);

                IElementHandle? input = null;
                foreach (var sel in SearchInputSelectors)
                {
                    var el = await page.QuerySelectorAsync(sel);
                    if (el != null && await el.IsVisibleAsync()) { input = el; break; }
                }
                if (input == null)
                    return JsonSerializer.Serialize(new { found = false, error = "no visible search input found on page" });

                lock (captured) captured.Clear();

                await input.FillAsync(probe);
                await input.PressAsync("Enter");

                var hit = await WaitForProbeRequestAsync(captured, probe, 8000);
                if (hit == null)
                {
                    // No request fired on Enter — try a visible submit/search button.
                    var btn = await page.QuerySelectorAsync(
                        "button[type=submit], input[type=submit], button[aria-label*=\"search\" i]");
                    if (btn != null && await btn.IsVisibleAsync())
                    {
                        await btn.ClickAsync();
                        hit = await WaitForProbeRequestAsync(captured, probe, 6000);
                    }
                }

                if (hit == null)
                    return JsonSerializer.Serialize(new { found = false, error = "search submitted but no request carried the probe term" });

                return JsonSerializer.Serialize(new
                {
                    found = true,
                    request = new
                    {
                        url = hit.Url,
                        method = hit.Method,
                        resourceType = hit.ResourceType,
                        postData = hit.PostData,
                        headers = hit.Headers,
                        responseContentType = hit.ResponseContentType
                    }
                });
            }
            finally
            {
                try { await page.CloseAsync(); } catch { }
            }
        }

        /// <summary>Poll the capture list until a request carries the probe
        /// term (raw or URL-encoded) in its URL or POST body.</summary>
        private static async Task<CapturedRequest?> WaitForProbeRequestAsync(
            List<CapturedRequest> captured, string probe, int timeoutMs)
        {
            var encoded = Uri.EscapeDataString(probe);
            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
            while (DateTime.UtcNow < deadline)
            {
                CapturedRequest? hit = null;
                lock (captured)
                {
                    // XHR/fetch first — the JS search endpoint; then any
                    // request (e.g. document nav) still carrying the term.
                    hit = captured.FirstOrDefault(r =>
                            (r.ResourceType == "xhr" || r.ResourceType == "fetch") && CarriesProbe(r, probe, encoded))
                        ?? captured.FirstOrDefault(r => CarriesProbe(r, probe, encoded));
                }
                if (hit != null) return hit;
                await Task.Delay(250);
            }
            return null;
        }

        private static bool CarriesProbe(CapturedRequest r, string probe, string encoded)
            => (!string.IsNullOrEmpty(r.Url) && (r.Url.Contains(probe) || r.Url.Contains(encoded)))
               || (!string.IsNullOrEmpty(r.PostData) && (r.PostData.Contains(probe) || r.PostData.Contains(encoded)));

        public void Dispose()
        {
            try { _context?.CloseAsync().GetAwaiter().GetResult(); } catch { }
            try { _playwright?.Dispose(); } catch { }
        }
    }
}
