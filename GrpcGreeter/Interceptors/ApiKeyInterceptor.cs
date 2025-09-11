namespace GrpcGreeter.Interceptors;

using Grpc.Core;
using Grpc.Core.Interceptors;

public class ApiKeyInterceptor : Interceptor
{
  private readonly string? _expectedApiKey;
  public ApiKeyInterceptor(IConfiguration configuration)
  {
   ArgumentNullException.ThrowIfNull(configuration);

    _expectedApiKey = configuration["ApiKey"];
  }

  public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
      TRequest request,
      ServerCallContext context,
      UnaryServerMethod<TRequest, TResponse> continuation)
  {
    var headers = context.RequestHeaders;
    var apiKeyHeader = headers.FirstOrDefault(h => h.Key == "x-api-key");

    if (apiKeyHeader == null || apiKeyHeader.Value != _expectedApiKey)
    {
      throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid API key"));
    }

    return await continuation(request, context);
  }
}
