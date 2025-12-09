using System.ComponentModel.DataAnnotations;

namespace AppServer.Models;

/// <summary>
/// Request model til at oprette et quiz spørgsmål.
/// Bruges til at definere spørgs mål tekst, rækkefølge, point og svarmuligheder.
/// </summary>
public class CreateQuizQuestionRequest
{
    /// <summary>
    /// Teksten til spørgsmålet
    /// </summary>
    [Required(ErrorMessage = "Question text is required")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Question text must be between 5 and 500 characters")]
    public string QuestionText { get; set; } = string.Empty;

    /// <summary>
    /// Rækkefølgen spørgsmålet vises i
    /// </summary>
    [Required(ErrorMessage = "Question order is required")]
    [Range(1, 1000, ErrorMessage = "Question order must be between 1 and 1000")]
    public int QuestionOrder { get; set; }

    /// <summary>
    /// Point givet for korrekt svar (standard 10)
    /// </summary>
    public int Points { get; set; } = 10;

    /// <summary>
    /// Liste af svarmuligheder
    /// </summary>
    [Required(ErrorMessage = "Options are required")]
    [MinLength(2, ErrorMessage = "At least 2 options are required")]
    [MaxLength(6, ErrorMessage = "Maximum 6 options allowed")]
    public List<CreateQuestionOptionRequest> Options { get; set; } = new();
}
