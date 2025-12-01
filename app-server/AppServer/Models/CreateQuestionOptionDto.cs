namespace AppServer.Models;

/// <summary>
/// DTO for creating a question option
/// </summary>
public class CreateQuestionOptionDto
{
    /// <summary>
    /// The text of the option
    /// </summary>
    public string OptionText { get; set; } = string.Empty;

    /// <summary>
    /// Whether this option is the correct answer
    /// </summary>
    public bool IsCorrect { get; set; }

    /// <summary>
    /// Display order of the option
    /// </summary>
    public int OptionOrder { get; set; }
}
