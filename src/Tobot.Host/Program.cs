using Tobot.Device;
using Tobot.Host.Components;
using Tobot.Host.Endpoints;
using Tobot.Host.Hubs;
using Tobot.Host.Services;
using Tobot.Shared;

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
app.MapHub<TelemetryHub>(TobotRoutes.TelemetryHub);
app.MapTobotApi();

// Release hardware (GPIO) cleanly on shutdown.
app.Lifetime.ApplicationStopping.Register(() => app.Services.GetRequiredService<TobotController>().Dispose());

app.Run();
