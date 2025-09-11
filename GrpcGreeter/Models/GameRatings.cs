namespace GrpcGreeter.Models;
public record GameRating
(
  uint Id,
  uint GameId,
  string Ip,
  uint Rating
);