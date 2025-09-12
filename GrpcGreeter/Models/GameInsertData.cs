namespace GrpcGreeter.Models;
public record GameInsertData
(
    string Name,
    uint ReleaseDate,
    uint PublisherId,
    uint DeveloperId
);
