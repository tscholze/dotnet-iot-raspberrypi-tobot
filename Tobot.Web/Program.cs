using Tobot.Web.Components;
using Tobot.Web.Hubs;
using Tobot.Device.ExplorerHat;

var builder = WebApplication.CreateBuilder(args);

builder.Services
	.AddRazorComponents()
	.AddInteractiveServerComponents();
builder.Services.AddSignalR();
builder.Services.AddSingleton<ExplorerHat>();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();
app.MapHub<TobotHub>("/tobothub");

app.Run();
