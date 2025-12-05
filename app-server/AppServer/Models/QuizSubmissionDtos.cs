namespace AppServer.Models;

/// <summary>
/// DTO for a single quiz answer (question -> selected option)
/// </summary>
public class QuizAnswerDto
{
    public Guid QuestionId { get; set; }
    public Guid SelectedOptionId { get; set; }
}

/// <summary>
/// Request model for submitting quiz answers
/// </summary>
public class SubmitQuizRequest
{
    public List<QuizAnswerDto> Answers { get; set; } = new();
    public int DurationSeconds { get; set; }
}

/// <summary>
/// DTO for quiz submission result
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
    
    // New fields for attempt tracking
    public string? AttemptId { get; set; }
    public bool IsNewBest { get; set; }
    public int PreviousBestScore { get; set; }
}

/// <summary>
/// DTO for individual answer result
/// </summary>
public class AnswerResultDto
{
    public Guid QuestionId { get; set; }
    public bool IsCorrect { get; set; }
    public int PointsEarned { get; set; }
}

/// <summary>
/// DTO for attempt summary
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
/// DTO for user statistics
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
