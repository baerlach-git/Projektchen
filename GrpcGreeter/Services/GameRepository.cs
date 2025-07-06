// Services/ProductRepository.cs
using Dapper;
using MySql.Data.MySqlClient; // Or use MySqlConnector.MySqlClient if using MySqlConnector
using System.Data;
using Bogus;
using Bogus.DataSets;

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

    public async Task<IEnumerable<Game>> GetAllAsync()
    {
      var query = "SELECT Id, Name, ReleaseDate, Publisher, DevStudio, Platform, Genre, CreatedAt, UpdatedAt FROM Games";  //probably will need some adaption in the future
      using var db = Connection;
      return await db.QueryAsync<Game>(query);
    }

    public async Task SeedDatabaseAsync()
    {
      using var db = Connection;

      // Create table if it doesn't exist
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
        Id INT AUTO_INCREMENT PRIMARY KEY,
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
        await db.ExecuteAsync(insertRatings);


      }



    }
  }
}
