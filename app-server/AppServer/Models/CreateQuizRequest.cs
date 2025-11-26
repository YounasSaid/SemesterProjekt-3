using System.ComponentModel.DataAnnotations;

namespace AppServer.Models;

/// <summary>
/// Request model for creating a quiz
/// </summary>
public class CreateQuizRequest
{
    /// <summary>
    /// The title of the quiz
    /// </summary>
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// List of questions in the quiz
    /// </summary>
    [Required(ErrorMessage = "Questions are required")]
    [MinLength(1, ErrorMessage = "At least 1 question is required")]
    public List<CreateQuizQuestionRequest> Questions { get; set; } = new();
}
