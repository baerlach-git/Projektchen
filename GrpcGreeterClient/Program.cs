using Grpc.Net.Client;
using Grpc.Core;
using GrpcGameService;
//server address http://188.245.118.112:5233
var channel = GrpcChannel.ForAddress("http://localhost:5233", new GrpcChannelOptions
{
    HttpHandler = new HttpClientHandler()
});

var gamesClient = new GameService.GameServiceClient(channel); //GamesServiceClient is an automatically generated class?

var headers = new Metadata
{
    { "x-api-key", "vwznnZRX1uX5dLDzw2RqGN1UevEHFoJjUGT6qcOCPLCLCa29VAlhBvbnpLA5fMBm" }
};



var gamesReply = await gamesClient.GetGamesAsync(new Empty { }, headers);
Console.WriteLine("these are our Games:");
Console.WriteLine("sry, there are too many games to print them all out");
//Console.Write(gamesReply.Games);

var ratingReply = await gamesClient.AddRatingAsync(new GameRatingRequest { GameId = 1, Ip = "5.61.144.202", Rating = 5 }, headers);
Console.WriteLine("Ratingreply:");
Console.WriteLine(ratingReply.Message);

Console.WriteLine("Press any key to exit...");
Console.ReadKey();