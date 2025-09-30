using Grpc.Core;
using Shared.Models;
using GrpcGreeter.Services;

namespace GrpcGreeter.Helpers;



public static class ExistanceChecker
{
    
    
    public static async Task CheckGameId(GameRepository repo, int gameId)
    {
        var gameExists = await repo.IdExistsInTableAsync(gameId, TableNames.Game);
        if (!gameExists)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "The game cannot be found"));
        }
    }

    public static async Task CheckCommentId(GameRepository repo, int? commentId, bool isParentCheck)
    {
        var commentExists = await repo.GameCommentExistsAsync(commentId);
        if(!commentExists)
        {
            string commentType = isParentCheck ? "parent comment" : "comment";
            throw new RpcException(new Status(StatusCode.NotFound, $"The {commentType} cannot be found"));
        }
    }
    
    
}