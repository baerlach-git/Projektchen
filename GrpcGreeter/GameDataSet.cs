using Bogus;
using Bogus.Premium;

namespace ExtendingBogus
{
  /// <summary>
  /// The following shows how to create a dedicated DataSet accessible via C# extension method.
  /// </summary>
  public static class ExtensionsForGames
  {
    public static GameData GameData(this Faker faker)
    {
      return ContextHelper.GetOrSet(faker, () => new GameData());
    }
  }

  /// <summary>
  /// This DatSet can be created manually using `new Candy()`, or by fluent extension method via <seealso cref="ExtensionsForFood"/>.
  /// </summary>
  public class GameData : DataSet
  {
    private static readonly string[] Genres =
       {
            "RPG", "Strategy", "RTS", "Shooter",
            "Beat'em Up", "Racing", "Platformer", "Point'n'Click"
         };

    /// <summary>
    /// Returns some type of candy.
    /// </summary>
    public string Genre()
    {
      return this.Random.ArrayElement(Genres);
    }

    private static readonly string[] Platforms = { "PC", "Playstation", "Xbox", "Switch", "Mobile" };
    public string Platform()
    {

      return this.Random.ArrayElement(Platforms);
    }
  }

}