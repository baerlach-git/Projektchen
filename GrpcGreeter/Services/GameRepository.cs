// Services/ProductRepository.cs
using Dapper;
using MySql.Data.MySqlClient; // Or use MySqlConnector.MySqlClient if using MySqlConnector
using System.Data;
using Bogus;
using Bogus.DataSets;
using System.Net.Http.Headers;

namespace ExtendingBogus
{

  public class GameRepository
  {
    private readonly string _connectionString;

    public GameRepository(IConfiguration config)
    {
      _connectionString = config.GetConnectionString("DefaultConnection");
    }

    private IDbConnection Connection => new MySqlConnection(_connectionString);

    public async Task<IEnumerable<Game>> GetGamesWithRatingsAsync()
    {
      var query = @"
        SELECT 
          g.Id, g.Name, g.ReleaseDate, g.Publisher, g.DevStudio, g.Platform, g.Genre, g.CreatedAt, g.UpdatedAt, 
          r.Id AS RatingId, r.GameId, r.Ip, r.Rating, r.Comment, r.CreatedAt AS RatingCreatedAt, r.UpdatedAt AS RatingUpdatedAt
        FROM Games g
        LEFT JOIN GameRatings r ON g.Id = r.GameId
        ";
      using var db = Connection;

      var GameDict = new Dictionary<int, Game>();
      //why <Game, GameRating, Game> ?
      var result = await db.QueryAsync<Game, GameRating, Game>(
        query,
        (game, rating) =>
        {
          if (!GameDict.TryGetValue(game.Id, out var currentGame))
          {
            currentGame = game;
            GameDict[game.Id] = currentGame;
            currentGame.Ratings = new List<GameRating>();
          }

          if (rating != null)
          {
            currentGame.Ratings.Add(rating);
          }

          return currentGame;
        },
        splitOn: "RatingId"
      );
      return GameDict.Values;
    }

    //TODO rethink if I want to keep this, maybe integrate check for ip in this/ also use this result for checking ip?
    public async Task<bool> GameExistsAsync(int gameId)
    {
      using var db = Connection;
      var sql = "SELECT COUNT(1) FROM Games WHERE Id = @Id";
      var result = await db.ExecuteScalarAsync<int>(sql, new { Id = gameId });
      return result > 0;
    }

    public async Task<bool> RatingExistsAsync(int gameId, string Ip)
    {
      using var db = Connection;
      var sql = @"
        SELECT COUNT(*) 
        FROM Games g 
        LEFT JOIN GameRatings r 
        ON g.Id = r.GameId
        WHERE g.Id = @Id AND r.Ip = @Ip";
      var result = await db.ExecuteScalarAsync<int>(sql, new { Id = gameId, Ip = Ip });
      return result > 0;

    }

    public async Task AddRatingAsync(GameRating rating)
    {
      using var db = Connection;
      var sql = @"
        INSERT INTO GameRatings (GameId, Ip, Rating, Comment)
        VALUES (@GameId, @Ip, @Rating, @Comment)";

      await db.ExecuteAsync(sql, rating);
    }

    public async Task UpdateRatingAsync(GameRating rating)
    {
      using var db = Connection;
      var sql = @"
        UPDATE GameRatings 
        SET Rating = @Rating, Comment = @Comment
        WHERE Id = @Id";
      await db.ExecuteAsync(sql, rating);

    }


    public async Task SeedDatabaseAsync()
    {
      using var db = Connection;

      var createGameTable = @"
        CREATE TABLE IF NOT EXISTS Games (
            Id INT AUTO_INCREMENT,
            Name VARCHAR(100) NOT NULL,
            ReleaseDate INT NOT NULL,
            Publisher VARCHAR(100) NOT NULL,
            DevStudio VARCHAR(100) NOT NULL,
            Platform VARCHAR(100) NOT NULL,
            Genre VARCHAR(100) NOT NULL,
            CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
            UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            PRIMARY KEY(Id)
        );";

      var createGameRatingTable = @"
        CREATE TABLE IF NOT EXISTS GameRatings (
          Id INT AUTO_INCREMENT,
          GameId INT NOT NULL,
          Ip VARCHAR(100) NOT NULL,
          Rating INT NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
          Comment VARCHAR(500) NOT NULL,
          CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
          UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
          PRIMARY KEY(Id),
          FOREIGN KEY (GameId) REFERENCES Games(Id)
        );";

      await db.ExecuteAsync(createGameTable);
      await db.ExecuteAsync(createGameRatingTable);

      // Seed data if table is empty TODO
      var gameCount = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Games");
      if (gameCount == 0)
      {
        var gameFaker = new Faker<Game>()
          .RuleFor(g => g.Name, f => f.Commerce.ProductName())
          .RuleFor(g => g.Publisher, f => f.Company.CompanyName())
          .RuleFor(g => g.DevStudio, f => f.Company.CompanyName())
          .RuleFor(g => g.Platform, f => f.GameData().Platform())
          .RuleFor(g => g.Genre, f => f.GameData().Genre())
          .RuleFor(g => g.ReleaseDate, f => f.Random.Int(1980, 2025));

        var fakeGames = gameFaker.Generate(100);

        var insertGames = "INSERT INTO Games (Name, ReleaseDate, Publisher, DevStudio, Platform, Genre) VALUES (@Name, @ReleaseDate, @Publisher, @DevStudio, @Platform, @Genre)";
        await db.ExecuteAsync(insertGames, fakeGames);
      }

      var gameIds = (await db.QueryAsync<int>("SELECT Id FROM Games")).ToList();


      var ratingsCount = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM GameRatings");
      if (ratingsCount == 0)
      {
        var ratingFaker = new Faker<GameRating>()
          .RuleFor(r => r.GameId, f => f.PickRandom(gameIds))
          .RuleFor(r => r.Ip, f => f.Internet.Ip())
          .RuleFor(r => r.Rating, f => f.Random.Int(1, 5))
          .RuleFor(r => r.Comment, f => f.Rant.Review("game"));

        var ratings = ratingFaker.Generate(1000);
        var insertRatings = @"
        INSERT INTO GameRatings (GameId, Ip, Rating, Comment) VALUES (@GameId, @Ip, @Rating, @Comment);";
        await db.ExecuteAsync(insertRatings, ratings);


      }

    }
  }
}
