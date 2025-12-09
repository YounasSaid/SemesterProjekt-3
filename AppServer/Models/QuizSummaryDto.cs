namespace AppServer.Models;

/// <summary>
/// DTO for quiz oversigt information.
/// Bruges til liste visninger uden fulde detaljer.
/// </summary>
public class QuizSummaryDto
{
    /// <summary>
    /// ID'et på quizzen
    /// </summary>
    public Guid QuizId { get; set; }

    /// <summary>
    /// Titlen på quizzen
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Antal spørgsmål i quizzen
    /// </summary>
    public int QuestionCount { get; set; }

    /// <summary>
    /// Total point tilgængelig i quizzen
    /// </summary>
    public int TotalPoints { get; set; }

    /// <summary>
    /// Hvornår quizzen blev oprettet
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
