using GrpcGreeter.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc(options =>
{
  options.Interceptors.Add<ApiKeyInterceptor>();
});

var app = builder.Build();

//app.UseHttpsRedirection(); didn't do anything

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.UseRouting();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
