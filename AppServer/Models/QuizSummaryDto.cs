namespace AppServer.Models;

/// <summary>
/// DTO for quiz summary information
/// </summary>
public class QuizSummaryDto
{
    /// <summary>
    /// The ID of the quiz
    /// </summary>
    public Guid QuizId { get; set; }

    /// <summary>
    /// The title of the quiz
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Number of questions in the quiz
    /// </summary>
    public int QuestionCount { get; set; }

    /// <summary>
    /// Total points available in the quiz
    /// </summary>
    public int TotalPoints { get; set; }

    /// <summary>
    /// When the quiz was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
