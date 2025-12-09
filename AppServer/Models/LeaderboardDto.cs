namespace AppServer.Models;

/// <summary>
/// DTO for en leaderboard entry (rangliste post).
/// Indeholder bruger information og statistik til visning af top spillere.
/// </summary>
public class LeaderboardEntryDto
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int Semester { get; set; }
    public int TotalScore { get; set; }
    public int QuizzesTaken { get; set; }
    public int Rank { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}
