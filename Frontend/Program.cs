using Frontend.Components;
using GameServiceProtos;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

//Needed to access http headers and thus ip addresses
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

/*var headerOptions = new ForwardedHeadersOptions
{
	ForwardedHeaders = ForwardedHeaders.All
};
headerOptions.KnownNetworks.Clear();
headerOptions.KnownProxies.Clear();
app.UseForwardedHeaders(headerOptions);
*/
app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
