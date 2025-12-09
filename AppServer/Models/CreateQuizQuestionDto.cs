namespace AppServer.Models;

/// <summary>
/// DTO til at oprette et quiz spørgsmål.
/// Bruges til kommunikation mellem AppServer og gRPC klienten.
/// </summary>
public class CreateQuizQuestionDto
{
    /// <summary>
    /// Teksten til spørgsmålet
    /// </summary>
    public string QuestionText { get; set; } = string.Empty;

    /// <summary>
    /// Rækkefølgen spørgsmålet vises i
    /// </summary>
    public int QuestionOrder { get; set; }

    /// <summary>
    /// Point givet for korrekt svar (altid 10)
    /// </summary>
    public int Points { get; set; } = 10;

    /// <summary>
    /// Liste af svarmuligheder
    /// </summary>
    public List<CreateQuestionOptionDto> Options { get; set; } = new();
}
