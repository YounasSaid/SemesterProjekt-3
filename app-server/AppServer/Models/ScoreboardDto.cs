namespace AppServer.Models;

/// <summary>
/// DTO for scoreboard entry
/// </summary>
public class ScoreboardEntryDto
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int TotalScore { get; set; }
    public int QuizzesTaken { get; set; }
}
