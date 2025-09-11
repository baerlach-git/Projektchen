using GrpcGreeter.Interceptors;
using GrpcGreeter.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc(options =>
{
  options.Interceptors.Add<ApiKeyInterceptor>();
}).AddServiceOptions<GameService>(opt => opt.EnableDetailedErrors = true);

builder.Services.AddSingleton<GameRepository>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
  var repo = scope.ServiceProvider.GetRequiredService<GameRepository>();
  await repo.SeedDatabaseAsync();
}

app.MapGrpcService<GameService>();

app.UseRouting();

app.Run();