namespace AppServer.Models;

/// <summary>
/// DTO for the result of creating a quiz
/// </summary>
public class CreateQuizResultDto
{
    /// <summary>
    /// The ID of the created quiz
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
}
