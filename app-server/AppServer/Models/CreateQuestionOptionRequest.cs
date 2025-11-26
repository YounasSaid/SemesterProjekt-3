using System.ComponentModel.DataAnnotations;

namespace AppServer.Models;

/// <summary>
/// Request model for creating a question option
/// </summary>
public class CreateQuestionOptionRequest
{
    /// <summary>
    /// The text of the option
    /// </summary>
    [Required(ErrorMessage = "Option text is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Option text must be between 1 and 200 characters")]
    public string OptionText { get; set; } = string.Empty;

    /// <summary>
    /// Whether this option is the correct answer
    /// </summary>
    [Required(ErrorMessage = "IsCorrect must be specified")]
    public bool IsCorrect { get; set; }

    /// <summary>
    /// Display order of the option
    /// </summary>
    [Required(ErrorMessage = "Option order is required")]
    [Range(1, 10, ErrorMessage = "Option order must be between 1 and 10")]
    public int OptionOrder { get; set; }
}
