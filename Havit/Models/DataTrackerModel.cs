namespace Havit.Models;

public class DataTrackerModel
{
    public bool IsAuthenticated { get; set; }
    public string? CurrentUsername { get; set; }
    public string? CurrentUserRole { get; set; }
}