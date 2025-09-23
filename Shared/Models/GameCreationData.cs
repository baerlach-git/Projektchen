using System.ComponentModel.DataAnnotations;


namespace Shared.Models;

public class GameCreationData
{
    public string? Name { get; set; }
    [Required(ErrorMessage = "A game name is required."), StringLength(100, MinimumLength = 1, ErrorMessage = "The name must be between 1 and 100 characters long.")]
    public int? ReleaseDate { get; set; }
    [Required(ErrorMessage = "A release date is required."), Range(1950, CurrentYear, ErrorMessage = "The release date must lie between 1950 and the current year.")]
    public int? PublisherId { get; set; }
    [Required(ErrorMessage = "A publisher is required.")]
    public int? DeveloperId { get; set; }
    [Required(ErrorMessage = "A developer is required.")]
    public int[]? GenreIds { get; set; }
    [Required(ErrorMessage = "At least one genre is required."), MinLength(1, ErrorMessage = "At least one genre is required.")]
    public int[]? PlatformIds { get; set; }
    [Required(ErrorMessage = "At least one platform is required."), MinLength(1, ErrorMessage = "At least one platform is required.")]
    
    //TODO find a way to get this dynamically
    private const int CurrentYear = 2025;
}