using System.ComponentModel.DataAnnotations;

namespace AppServer.Models;

/// <summary>
/// Request model til at oprette en quiz.
/// Indeholder quiz titel og liste af spørgsmål med svarmuligheder.
/// </summary>
public class CreateQuizRequest
{
    /// <summary>
    /// Titlen på quizzen
    /// </summary>
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Liste af spørgsmål i quizzen
    /// </summary>
    [Required(ErrorMessage = "Questions are required")]
    [MinLength(1, ErrorMessage = "At least 1 question is required")]
    public List<CreateQuizQuestionRequest> Questions { get; set; } = new();
}
