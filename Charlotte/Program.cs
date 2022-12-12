using Charlotte;
using System.Text.Json;

Chrome.Browser = new Browser();

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders().AddProvider(new CharlotteLoggerProvider(null));

// Add services to the container.
builder.Services.AddControllers();
var app = builder.Build();
app.UseAuthorization();

//load blacklist.json
if (File.Exists(App.MapPath("blacklist.json")))
{
    Settings.BlacklistedDomains = JsonSerializer.Deserialize<string[]>(File.ReadAllText(App.MapPath("blacklist.json"))) ?? new string[] { };
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Browser}/{action=Index}");

app.Run();
