using Bogus;
using Bogus.Premium;

namespace ExtendingBogus
{

  public static class ExtensionsForGames
  {
    public static GameData GameData(this Faker faker)
    {
      return ContextHelper.GetOrSet(faker, () => new GameData());
    }
  }


  public class GameData : DataSet
  {
    private static readonly string[] Genres =
       {
            "RPG", "Strategy", "RTS", "Shooter",
            "Beat'em Up", "Racing", "Platformer", "Point'n'Click"
         };


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