using Grpc.Net.Client;
using GrpcGreeterClient;
using Grpc.Core;


// The port number must match the port of the gRPC server.
//tutorial code
//using var channel = GrpcChannel.ForAddress("https://localhost:5233"); 

//chtgpt solution to ssl problems
var channel = GrpcChannel.ForAddress("http://localhost:5233", new GrpcChannelOptions
{
    HttpHandler = new HttpClientHandler()
});
var client = new Greeter.GreeterClient(channel);

var headers = new Metadata
{
    { "x-api-key", "vwznnZRX1uX5dLDzw2RqGN1UevEHFoJjUGT6qcOCPLCLCa29VAlhBvbnpLA5fMBm" }
};

var reply = await client.SayHelloAsync(
    new HelloRequest { Name = "GreeterClient" }, headers);
Console.WriteLine("Greeting: " + reply.Message);
Console.WriteLine("Press any key to exit...");
Console.ReadKey();