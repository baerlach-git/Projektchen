using Grpc.Net.Client;
using GrpcGreeterClient;
using Grpc.Core;
using GrpcGameService;


// The port number must match the port of the gRPC server.
//tutorial code
//using var channel = GrpcChannel.ForAddress("https://localhost:5233"); 

//chtgpt solution to ssl problems
var channel = GrpcChannel.ForAddress("http://localhost:5233", new GrpcChannelOptions
{
    HttpHandler = new HttpClientHandler()
});
var greeterClient = new Greeter.GreeterClient(channel);
var gamesClient = new GameService.GameServiceClient(channel); //GamesServiceClient is an automatically generated class?

var headers = new Metadata
{
    { "x-api-key", "vwznnZRX1uX5dLDzw2RqGN1UevEHFoJjUGT6qcOCPLCLCa29VAlhBvbnpLA5fMBm" }
};

var greeterReply = await greeterClient.SayHelloAsync(
    new HelloRequest { Name = "GreeterClient" }, headers);
Console.WriteLine("Greeting: " + greeterReply.Message);


var gamesReply = await gamesClient.GetGamesAsync(new Empty { }, headers);
Console.WriteLine("these are our Games:");
Console.WriteLine("sry, there are too many games to print them all out");
//Console.Write(gamesReply.Games);

var ratingReply = await gamesClient.AddRatingAsync(new GameRatingRequest { GameId = 1, Ip = "5.61.144.202", Rating = 5, Comment = "" }, headers);
Console.WriteLine("Ratingreply:");
Console.WriteLine(ratingReply.Message);




Console.WriteLine("Press any key to exit...");
Console.ReadKey();