using Charlotte;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsService();
builder.Logging.ClearProviders().AddProvider(new CharlotteLoggerProvider(null));

// Add services to the container.
builder.Services.AddControllers();
var app = builder.Build();
app.UseAuthorization();

// Initialize browser backends after app setup. Config "Browser" picks the
// primary ("cefsharp" | "playwright"); the other stays warm as the per-request
// fallback inside CompositeBrowser. Both are registered in Chrome.Backends so
// a request can force one with ?backend= for A/B testing.
var primaryName = (builder.Configuration["Browser"] ?? "cefsharp").ToLowerInvariant();
if (primaryName != "playwright") primaryName = "cefsharp";
var fallbackName = primaryName == "cefsharp" ? "playwright" : "cefsharp";

foreach (var name in new[] { "cefsharp", "playwright" })
{
    if (Chrome.Backends.ContainsKey(name)) continue;
    try
    {
        Chrome.Backends[name] = name == "playwright"
            ? (IBrowser)PlaywrightBrowser.Create()
            : new Browser();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR initializing {name} backend: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
    }
}

if (Chrome.Backends.TryGetValue(primaryName, out var primary))
{
    Chrome.Backends.TryGetValue(fallbackName, out var secondary);
    Chrome.Browser = new CompositeBrowser(primary, primaryName, secondary, fallbackName);
    Console.WriteLine($"Browser backend: {primaryName} (fallback: {(secondary != null ? fallbackName : "none — init failed")})");
}
else if (Chrome.Backends.TryGetValue(fallbackName, out var only))
{
    Chrome.Browser = only;
    Console.WriteLine($"Browser backend: {fallbackName} only ({primaryName} failed to initialize)");
}
else
{
    Console.WriteLine("ERROR: no browser backend initialized — Charlotte cannot serve requests");
}

//load blacklist.json
if (File.Exists(App.MapPath("blacklist.json")))
{
    Settings.BlacklistedDomains = JsonSerializer.Deserialize<string[]>(File.ReadAllText(App.MapPath("blacklist.json"))) ?? new string[] { };
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Browser}/{action=Index}");

app.Run();
