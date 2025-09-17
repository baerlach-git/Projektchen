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


//GETTING ALL GAMES
var gamesReply = await gamesClient.GetGamesAsync(new EmptyMessage { }, headers);
Console.WriteLine("these are some of our Games:");
foreach (var game in gamesReply.Games.Take(5))
{
    Console.WriteLine(game);
}
//ADDING A RATING
var ratingReply = await gamesClient.AddRatingAsync(new GameRatingRequest { GameId = gamesReply.Games.First().Id, Rating = 4 }, headers);
Console.WriteLine("Ratingreply:");
Console.WriteLine(ratingReply.Message);

//ADDING A COMMENT                              AddGameComment
var addCommentReply = await gamesClient.AddGameCommentAsync(new AddGameCommentRequest { GameId = gamesReply.Games.First().Id, Content = "das ist ein tolles Spiel", ParentId = null}, headers);
Console.WriteLine("addCommentReply:");
Console.WriteLine(addCommentReply.Message);

//GETTING ALL COMMENTS
var getAllCommentsReply = await gamesClient.GetAllCommentsForGameAsync(new GetAllCommentsForGameRequest{GameId = gamesReply.Games.First().Id}, headers);
foreach (var comment in getAllCommentsReply.GameComments)
{
    Console.WriteLine($"{comment.Content} {comment.CreatedAt}");    
}
//UPDATING A COMMENT
string commentUpdate = "das ist der neue Kommentar";
var updateCommentReply = await gamesClient.UpdateGameCommentAsync(new UpdateGameCommentRequest{CommentId = getAllCommentsReply.GameComments.First().Id, Content = commentUpdate}, headers);
Console.WriteLine("updating first comment:");
Console.WriteLine(updateCommentReply.Message);
Console.WriteLine("updated content");
Console.WriteLine(commentUpdate);
var getAllCommentsAgain = await gamesClient.GetAllCommentsForGameAsync(new GetAllCommentsForGameRequest{GameId = gamesReply.Games.First().Id}, headers);
Console.WriteLine("firstComment after Update");
var updatedComment = getAllCommentsAgain.GameComments.First();
Console.WriteLine("updated first comment:");
Console.WriteLine(updatedComment.Content, updatedComment.CreatedAt,  updatedComment.UpdatedAt, updatedComment.Edited);


//DELETING A COMMENT
var deleteFirstGameCommentWithoutParent =
    await gamesClient.DeleteGameCommentAsync(
        new DeleteGameCommentRequest { CommentId = getAllCommentsAgain.GameComments.First().Id, ParentId = null}, headers);
Console.WriteLine("delete first comment without parent");
Console.WriteLine(deleteFirstGameCommentWithoutParent.Message);
var deleteLastGameCommentWithParent =
    await gamesClient.DeleteGameCommentAsync(
        new DeleteGameCommentRequest { CommentId = getAllCommentsAgain.GameComments.Last().Id, ParentId = 5}, headers);
Console.WriteLine("delete last comment with parent");
Console.WriteLine(deleteLastGameCommentWithParent.Message);
var gettingAllCommentsThirdTime = await gamesClient.GetAllCommentsForGameAsync(new  GetAllCommentsForGameRequest{GameId = gamesReply.Games.First().Id}, headers);
foreach (var comment in gettingAllCommentsThirdTime.GameComments)
{
    Console.WriteLine($"{comment.Content} {comment.CreatedAt} {comment.Deleted}");
}


Console.WriteLine("Press any key to exit...");
Console.ReadKey();