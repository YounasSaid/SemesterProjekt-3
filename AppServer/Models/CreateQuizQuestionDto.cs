namespace AppServer.Models;

/// <summary>
/// DTO for creating a quiz question
/// </summary>
public class CreateQuizQuestionDto
{
    /// <summary>
    /// The text of the question
    /// </summary>
    public string QuestionText { get; set; } = string.Empty;

    /// <summary>
    /// Display order of the question
    /// </summary>
    public int QuestionOrder { get; set; }

    /// <summary>
    /// Points awarded for correct answer (always 10)
    /// </summary>
    public int Points { get; set; } = 10;

    /// <summary>
    /// List of answer options
    /// </summary>
    public List<CreateQuestionOptionDto> Options { get; set; } = new();
}
