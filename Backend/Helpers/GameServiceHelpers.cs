using Grpc.Core;

namespace GrpcGreeter.Helpers;

public static class GameServiceHelpers
{
    public static string GetClientIp(ServerCallContext context)
    {
        var httpContext = context.GetHttpContext();
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString();

        if (clientIp == null)
        {
            //note sure which error code would be reasonable, RemoteIpAddress is only null for non TCP connections.
            //Is this case even possible considering I control the client/ in a GRPC Client in general?
            throw new RpcException(new Status(StatusCode.Aborted, "Only TCP connections are accepted"));
        }
        return clientIp;
    }
}