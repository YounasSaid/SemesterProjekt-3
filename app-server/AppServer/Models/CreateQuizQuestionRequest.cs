using System.ComponentModel.DataAnnotations;

namespace AppServer.Models;

/// <summary>
/// Request model for creating a quiz question
/// </summary>
public class CreateQuizQuestionRequest
{
    /// <summary>
    /// The text of the question
    /// </summary>
    [Required(ErrorMessage = "Question text is required")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Question text must be between 5 and 500 characters")]
    public string QuestionText { get; set; } = string.Empty;

    /// <summary>
    /// Display order of the question
    /// </summary>
    [Required(ErrorMessage = "Question order is required")]
    [Range(1, 1000, ErrorMessage = "Question order must be between 1 and 1000")]
    public int QuestionOrder { get; set; }

    /// <summary>
    /// Points awarded for correct answer (default 10)
    /// </summary>
    public int Points { get; set; } = 10;

    /// <summary>
    /// List of answer options
    /// </summary>
    [Required(ErrorMessage = "Options are required")]
    [MinLength(2, ErrorMessage = "At least 2 options are required")]
    [MaxLength(6, ErrorMessage = "Maximum 6 options allowed")]
    public List<CreateQuestionOptionRequest> Options { get; set; } = new();
}
