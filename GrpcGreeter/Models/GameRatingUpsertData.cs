namespace GrpcGreeter.Models;
public record GameRatingUpsertData
(
  uint GameId,
  string Ip,
  uint Rating
);