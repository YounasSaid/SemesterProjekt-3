namespace AppServer.Models;

/// <summary>
/// DTO for resultatet af at oprette en quiz.
/// Returneres til klienten efter succesfuld quiz oprettelse.
/// </summary>
public class CreateQuizResultDto
{
    /// <summary>
    /// ID'et på den oprettede quiz
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
}
