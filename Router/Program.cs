using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.Name = ".Charlottes.Web.Router";
    options.Cookie.IsEssential = true;
});
builder.Services.AddSignalR();
builder.Services.AddMvc().AddRazorRuntimeCompilation();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSession();
//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();


//check if app is running in Docker Container
Router.App.IsDocker = System.Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

switch (app.Environment.EnvironmentName.ToLower())
{
    case "production":
        Router.App.Environment = Router.Environment.production;
        break;
    case "staging":
        Router.App.Environment = Router.Environment.staging;
        break;
    default:
        Router.App.Environment = Router.Environment.development;
        break;
}

//load application-wide cache
Router.App.ConfigFilename = "config" +
    (Router.App.IsDocker ? ".docker" : "") +
    (Router.App.Environment == Router.Environment.production ? ".prod" : "") + ".json";

var builtConfig = new ConfigurationBuilder()
                .AddJsonFile(Router.App.MapPath(Router.App.ConfigFilename))
                .AddEnvironmentVariables().Build();
builtConfig.Bind(Router.App.Config);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.MapHub<Router.SignalR.DashboardHub>("/dashboardhub");

app.Start();

//get IP addresses for running application
var server = app.Services.GetRequiredService<IServer>();
var addressFeature = server.Features.Get<IServerAddressesFeature>();
if (addressFeature != null)
{
    foreach (var address in addressFeature.Addresses)
    {
        Console.WriteLine($"Listening to {address}");
        Router.App.Addresses.Add(address);
    }
}

app.WaitForShutdown();