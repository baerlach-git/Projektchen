namespace Shared.Models;

public record GameCommentDto(
    int Id,
    int GameId,
    int? ParentId,
    string Ip,
    string Content, 
    bool Deleted,
    bool Edited,
    DateTime CreatedAt,
    DateTime UpdatedAt

    );
    
    
    //(System.Int32 Id, System.Int32 GameId, System.Int32 ParentId, System.String Ip, System.String Content, System.Boolean Deleted, System.Boolean Edited, System.DateTime CreatedAt, System.DateTime UpdatedAt) is required for Backend.Models.GameCommentDto materiali