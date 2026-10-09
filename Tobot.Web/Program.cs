using Tobot.Web.Components;
using Tobot.Web.Hubs;
using Tobot.Device;
using Tobot.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
	.AddRazorComponents()
	.AddInteractiveServerComponents();
builder.Services.AddSignalR();
builder.Services.AddSingleton<TobotController>();
builder.Services.AddHostedService<DistanceBroadcastService>();
builder.Services.AddHostedService<PiStatusBroadcastService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();
app.MapHub<TobotHub>("/tobothub");

app.Run();
