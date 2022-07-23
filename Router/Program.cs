var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

//check if app is running in Docker Container
Router.App.IsDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

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
var configFile = "config" +
    (Router.App.IsDocker ? ".docker" : "") +
    (Router.App.Environment == Router.Environment.production ? ".prod" : "") + ".json";

var builtConfig = new ConfigurationBuilder()
                .AddJsonFile(Router.App.MapPath(configFile))
                .AddEnvironmentVariables().Build();
builtConfig.Bind(Router.App.Config);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
