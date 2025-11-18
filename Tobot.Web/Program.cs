using Tobot.Web.Components;
using Tobot.Web.Hubs;
using Tobot.Device.ExplorerHat;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();
builder.Services.AddSignalR();
builder.Services.AddSingleton<ExplorerHat>();

var app = builder.Build();

app.UseAntiforgery();

app.MapRazorComponents<App>();
app.MapHub<TobotHub>("/tobothub");

app.Run();
