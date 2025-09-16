using Grpc.Net.Client;
using Grpc.Core;
using GameServiceProtos;
//server address: http://188.245.118.112:5233
//local address: http://localhost:5233
//docker container: 172.18.0.3
var channel = GrpcChannel.ForAddress("http://172.18.0.3:5233", new GrpcChannelOptions
{
    HttpHandler = new HttpClientHandler()
});

var gamesClient = new GameService.GameServiceClient(channel);

var headers = new Metadata
{
    { "x-api-key", "vwznnZRX1uX5dLDzw2RqGN1UevEHFoJjUGT6qcOCPLCLCa29VAlhBvbnpLA5fMBm" }
};



var gamesReply = await gamesClient.GetGamesAsync(new EmptyMessage { }, headers);
Console.WriteLine("these are some of our Games:");
foreach (var game in gamesReply.Games.Take(5))
{
    Console.WriteLine(game);
}

var ratingReply = await gamesClient.AddRatingAsync(new GameRatingRequest { GameId = 579, Rating = 4 }, headers);
Console.WriteLine("Ratingreply:");
Console.WriteLine(ratingReply.Message);

Console.WriteLine("Press any key to exit...");
Console.ReadKey();