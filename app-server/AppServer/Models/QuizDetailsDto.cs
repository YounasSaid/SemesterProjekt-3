namespace AppServer.Models;

public class QuizDetailsDto
{
    public Guid QuizId { get; set; }
    public string Title { get; set; } = "";
    public List<QuestionDto> Questions { get; set; } = new();
    public int TotalPoints { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class QuestionDto
{
    public Guid QuestionId { get; set; }
    public string QuestionText { get; set; } = "";
    public int QuestionOrder { get; set; }
    public int Points { get; set; }
    public List<QuestionOptionDto> Options { get; set; } = new();
}

public class QuestionOptionDto
{
    public Guid OptionId { get; set; }
    public string OptionText { get; set; } = "";
    public int OptionOrder { get; set; }
    public bool IsCorrect { get; set; }
}
