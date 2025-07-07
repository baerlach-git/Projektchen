using GrpcGreeter.Services;

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

//app.UseHttpsRedirection(); didn't do anything

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<GameService>();

app.UseRouting();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
