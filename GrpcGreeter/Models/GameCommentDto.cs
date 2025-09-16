using Google.Protobuf.WellKnownTypes;

namespace GrpcGreeter.Models;

public record GameCommentDto(
    int Id,
    int GameId,
    int? ParentId,
    string Ip,
    string Content, 
    bool Deleted,
    bool Edited,
    Timestamp CreatedAt,
    Timestamp UpdatedAt

    );