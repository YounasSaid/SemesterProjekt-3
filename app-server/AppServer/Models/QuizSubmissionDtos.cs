namespace AppServer.Models;

/// <summary>
/// DTO for a single quiz answer (question -> selected option)
/// </summary>
public class QuizAnswerDto
{
    /// <summary>
    /// The ID of the question being answered
    /// </summary>
    public Guid QuestionId { get; set; }

    /// <summary>
    /// The ID of the selected option
    /// </summary>
    public Guid SelectedOptionId { get; set; }
}

/// <summary>
/// Request model for submitting quiz answers
/// </summary>
public class SubmitQuizRequest
{
    /// <summary>
    /// List of answers (one per question)
    /// </summary>
    public List<QuizAnswerDto> Answers { get; set; } = new();
}

/// <summary>
/// DTO for quiz submission result
/// </summary>
public class QuizSubmissionResultDto
{
    /// <summary>
    /// Whether the submission was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Score achieved (points earned)
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Total possible points
    /// </summary>
    public int TotalPoints { get; set; }

    /// <summary>
    /// Percentage score (0-100)
    /// </summary>
    public double Percentage { get; set; }

    /// <summary>
    /// Number of correct answers
    /// </summary>
    public int CorrectCount => Results.Count(r => r.IsCorrect);

    /// <summary>
    /// Total number of questions
    /// </summary>
    public int TotalCount => Results.Count;

    /// <summary>
    /// Detailed results per question
    /// </summary>
    public List<AnswerResultDto> Results { get; set; } = new();
}

/// <summary>
/// DTO for individual answer result
/// </summary>
public class AnswerResultDto
{
    /// <summary>
    /// The ID of the question
    /// </summary>
    public Guid QuestionId { get; set; }

    /// <summary>
    /// Whether the answer was correct
    /// </summary>
    public bool IsCorrect { get; set; }

    /// <summary>
    /// Points earned for this question
    /// </summary>
    public int PointsEarned { get; set; }
}
