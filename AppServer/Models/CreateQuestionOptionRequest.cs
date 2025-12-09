using System.ComponentModel.DataAnnotations;

namespace AppServer.Models;

/// <summary>
/// Request model til at oprette en svar mulighed.
/// Definerer svarmulighed tekst, om den er korrekt, og rækkefølge.
/// </summary>
public class CreateQuestionOptionRequest
{
    /// <summary>
    /// Teksten til svar muligheden
    /// </summary>
    [Required(ErrorMessage = "Option text is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Option text must be between 1 and 200 characters")]
    public string OptionText { get; set; } = string.Empty;

    /// <summary>
    /// Om denne svarmulighed er det korrekte svar
    /// </summary>
    [Required(ErrorMessage = "IsCorrect must be specified")]
    public bool IsCorrect { get; set; }

    /// <summary>
    /// Rækkefølgen svarmuligheden vises i
    /// </summary>
    [Required(ErrorMessage = "Option order is required")]
    [Range(1, 10, ErrorMessage = "Option order must be between 1 and 10")]
    public int OptionOrder { get; set; }
}
