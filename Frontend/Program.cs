using Frontend.Components;
using GameServiceProtos;
using MudBlazor.Services;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();
builder.Services.AddHttpContextAccessor();
//not really necessary in this case, only needed when deploying on remote containers like azure containers and
//running multiple instances of the server app on several containers at once
builder.Services.AddDataProtection();

builder.Services.AddGrpcClient<GameService.GameServiceClient>(o =>
{
    o.Address = new Uri("http://172.18.0.3:5233");
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
