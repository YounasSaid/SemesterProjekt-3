namespace AppServer.Models;

/// <summary>
/// DTO for et enkelt quiz svar (spørgsmål -> valgt svarmulighed).
/// </summary>
public class QuizAnswerDto
{
    public Guid QuestionId { get; set; }
    public Guid SelectedOptionId { get; set; }
}

/// <summary>
/// Request model til indsendelse af quiz svar.
/// </summary>
public class SubmitQuizRequest
{
    public List<QuizAnswerDto> Answers { get; set; } = new();
    public int DurationSeconds { get; set; }
}

/// <summary>
/// DTO for quiz indsendelse resultat.
/// Indeholder score, procent og individuelle svar resultater.
/// </summary>
public class QuizSubmissionResultDto
{
    public bool Success { get; set; }
    public int Score { get; set; }
    public int TotalPoints { get; set; }
    public double Percentage { get; set; }
    public int CorrectCount => Results.Count(r => r.IsCorrect);
    public int TotalCount => Results.Count;
    public List<AnswerResultDto> Results { get; set; } = new();

    // Nye felter til forsøgs tracking
    public string? AttemptId { get; set; }
    public bool IsNewBest { get; set; }
    public int PreviousBestScore { get; set; }
}

/// <summary>
/// DTO for individuelt svar resultat.
/// </summary>
public class AnswerResultDto
{
    public Guid QuestionId { get; set; }
    public bool IsCorrect { get; set; }
    public int PointsEarned { get; set; }
}

/// <summary>
/// DTO for forsøgs oversigt.
/// Bruges til at vise historik over gennemførte quizzer.
/// </summary>
public class AttemptSummaryDto
{
    public Guid AttemptId { get; set; }
    public Guid QuizId { get; set; }
    public string QuizTitle { get; set; } = "";
    public int Score { get; set; }
    public int TotalPoints { get; set; }
    public int CorrectCount { get; set; }
    public int TotalCount { get; set; }
    public int DurationSeconds { get; set; }
    public DateTime CompletedAt { get; set; }
    public bool IsBestAttempt { get; set; }
    public double Percentage { get; set; }

    public string FormattedDuration
    {
        get
        {
            var ts = TimeSpan.FromSeconds(DurationSeconds);
            if (ts.TotalMinutes >= 1)
                return $"{(int)ts.TotalMinutes}m {ts.Seconds}s";
            return $"{ts.Seconds}s";
        }
    }
}

/// <summary>
/// DTO for bruger statistik.
/// Samlet oversigt over brugerens quiz præstationer.
/// </summary>
public class UserStatsDto
{
    public int TotalScore { get; set; }
    public int QuizzesTaken { get; set; }
    public int TotalAttempts { get; set; }
    public int QuizzesCreated { get; set; }
    public double AveragePercentage { get; set; }
    public List<AttemptSummaryDto> RecentAttempts { get; set; } = new();
}
