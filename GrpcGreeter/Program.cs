var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc(options =>
{
  options.Interceptors.Add<ApiKeyInterceptor>();
}).AddServiceOptions<GameService>(opt => opt.EnableDetailedErrors = true);

builder.Services.AddSingleton<ExtendingBogus.GameRepository>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
  var repo = scope.ServiceProvider.GetRequiredService<ExtendingBogus.GameRepository>();
  await repo.SeedDatabaseAsync();
}

app.MapGrpcService<GameService>();

app.UseRouting();

app.Run();
