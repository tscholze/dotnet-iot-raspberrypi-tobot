using Tobot.Web.Components;
using Tobot.Web.Hubs;
using Tobot.Device.ExplorerHat;
using Tobot.Device;
using Tobot.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
	.AddRazorComponents()
	.AddInteractiveServerComponents();
builder.Services.AddSignalR();
builder.Services.AddSingleton<ExplorerHat>();
builder.Services.AddSingleton<TobotController>();
builder.Services.AddHostedService<DistanceBroadcastService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();
app.MapHub<TobotHub>("/tobothub");

app.Run();
