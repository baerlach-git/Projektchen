using Grpc.Net.Client;
using Grpc.Core;
using GrpcGameService;
//server address http://188.245.118.112:5233
var channel = GrpcChannel.ForAddress("http://localhost:5233", new GrpcChannelOptions
{
    HttpHandler = new HttpClientHandler()
});

var gamesClient = new GameService.GameServiceClient(channel);

var headers = new Metadata
{
    { "x-api-key", "vwznnZRX1uX5dLDzw2RqGN1UevEHFoJjUGT6qcOCPLCLCa29VAlhBvbnpLA5fMBm" }
};



var gamesReply = await gamesClient.GetGamesAsync(new Empty { }, headers);
Console.WriteLine("these are some of our Games:");
foreach (var game in gamesReply.Games.Where(g => g.Id <= 5))
{
    Console.WriteLine(game);
}

var ratingReply = await gamesClient.AddRatingAsync(new GameRatingRequest { GameId = 1, Rating = 5 }, headers);
Console.WriteLine("Ratingreply:");
Console.WriteLine(ratingReply.Message);

Console.WriteLine("Press any key to exit...");
Console.ReadKey();