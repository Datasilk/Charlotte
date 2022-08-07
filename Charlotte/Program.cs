using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
var app = builder.Build();
app.UseAuthorization();

//load blacklist.json
if (File.Exists(Charlotte.App.MapPath("blacklist.json")))
{
    Charlotte.Settings.BlacklistedDomains = JsonSerializer.Deserialize<string[]>(File.ReadAllText(Charlotte.App.MapPath("blacklist.json"))) ?? new string[] { };
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Browser}/{action=Index}");

app.Run();
