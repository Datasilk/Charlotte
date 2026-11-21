namespace Charlotte
{
    public static class Chrome
    {
        /// <summary>Default request path — CompositeBrowser (primary+fallback).</summary>
        public static IBrowser Browser { get; set; }

        /// <summary>All initialized backends by name — lets a request pin one
        /// with ?backend=cefsharp|playwright for A/B testing.</summary>
        public static Dictionary<string, IBrowser> Backends { get; } =
            new(StringComparer.OrdinalIgnoreCase);
    }
}
