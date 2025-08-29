using Charlotte;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders().AddProvider(new CharlotteLoggerProvider(null));

// Add services to the container.
builder.Services.AddControllers();
var app = builder.Build();
app.UseAuthorization();

// Initialize browser after app setup
try
{
    Chrome.Browser = new Browser();
}
catch (Exception ex)
{
    Console.WriteLine($"ERROR initializing Browser: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    // Continue execution to see if we can get more diagnostic information
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
