using GameServiceProtos;
using Shared.Models;

namespace Frontend.Helpers;

public static class Mapper
{
    public static PublisherDto MapToPublisherDto(this Publisher publisher)
    {
        return new PublisherDto(publisher.Id,publisher.Name);
    }
    
    public static DeveloperDto MapToDeveloperDto(this Developer developer)
    {
        return new DeveloperDto(developer.Id,developer.Name);
    }
    
    public static PlatformDto MapToPlatformDto(this Platform platform)
    {
        return new PlatformDto(platform.Id,platform.Name);
    }
    
    public static GenreDto MapToGenreDto(this Genre genre)
    {
        return new GenreDto(genre.Id,genre.Name);
    }
}