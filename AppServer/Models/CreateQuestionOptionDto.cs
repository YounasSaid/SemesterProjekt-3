namespace AppServer.Models;

/// <summary>
/// DTO til at oprette en svarmulighed.
/// Bruges til kommunikation mellem AppServer og gRPC klienten.
/// </summary>
public class CreateQuestionOptionDto
{
    /// <summary>
    /// Teksten til svarmuligheden
    /// </summary>
    public string OptionText { get; set; } = string.Empty;

    /// <summary>
    /// Om denne svarmulighed er det korrekte svar
    /// </summary>
    public bool IsCorrect { get; set; }

    /// <summary>
    /// Rækkefølgen svarmuligheden vises i
    /// </summary>
    public int OptionOrder { get; set; }
}
