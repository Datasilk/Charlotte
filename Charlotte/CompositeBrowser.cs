namespace Charlotte
{
    /// <summary>
    /// Wraps a primary backend with a graceful fallback — a failed Collect
    /// ("error:" exception, "log:" nav-timeout) retries on the secondary
    /// backend before giving up. Backend init failures simply leave that
    /// backend out of the chain.
    /// </summary>
    public class CompositeBrowser : IBrowser
    {
        private readonly IBrowser _primary;
        private readonly IBrowser? _fallback;
        private readonly string _primaryName;
        private readonly string _fallbackName;

        public CompositeBrowser(IBrowser primary, string primaryName, IBrowser? fallback, string fallbackName)
        {
            _primary = primary;
            _primaryName = primaryName;
            _fallback = fallback;
            _fallbackName = fallbackName;
        }

        /// <summary>A DOM-JSON payload smaller than this is almost certainly a
        /// bot-lite stub (PMC serves ~30-element shells) rather than a real
        /// page — worth one retry on the other backend, whose different
        /// fingerprint may get the full render. Real pages are 100KB+.</summary>
        private const int ThinDomChars = 15000;

        public string Collect(string url)
        {
            var result = SafeCollect(_primary, _primaryName, url);
            if (_fallback == null) return result;
            if (!IsFailure(result) && result.Length >= ThinDomChars) return result;

            Console.WriteLine($"{_primaryName} backend {(IsFailure(result) ? "failed" : "returned a thin DOM")} ({Snippet(result)}) — retrying on {_fallbackName}");
            var retry = SafeCollect(_fallback, _fallbackName, url);
            if (IsFailure(retry)) return result; // report the first error when both fail

            // Thin-DOM case: keep whichever backend produced the larger DOM.
            return IsFailure(result) || retry.Length >= result.Length ? retry : result;
        }

        private static string SafeCollect(IBrowser browser, string name, string url)
        {
            try { return browser.Collect(url); }
            catch (Exception ex) { return $"error: {name} threw: {ex.Message}"; }
        }

        /// <summary>CefSharp reports failures as "error:"/"log:" prefixed text
        /// ("file:" is a real result — a downloadable, not an HTML page).</summary>
        private static bool IsFailure(string? result)
            => string.IsNullOrWhiteSpace(result)
               || result.StartsWith("error:", StringComparison.OrdinalIgnoreCase)
               || result.StartsWith("log:", StringComparison.OrdinalIgnoreCase);

        private static string Snippet(string s)
            => s.Length > 120 ? s.Substring(0, 120) + "…" : s.Replace('\n', ' ');
    }
}
