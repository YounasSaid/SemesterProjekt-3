using AppServer.Models;
using AppServer.Services;
using AppServer.Utils;
using Microsoft.AspNetCore.Mvc;

namespace AppServer.Controllers;

/// <summary>
/// REST API controller for quiz management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class QuizManagementController : ControllerBase
{
    private readonly QuizGrpcClient _quizClient;
    private readonly ILogger<QuizManagementController> _logger;

    public QuizManagementController(QuizGrpcClient quizClient, ILogger<QuizManagementController> logger)
    {
        _quizClient = quizClient;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new quiz with questions and options
    /// </summary>
    /// <param name="request">Quiz creation request with title and questions</param>
    /// <returns>Created quiz information</returns>
    /// <response code="201">Quiz created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("create")]
    [ProducesResponseType(typeof(CreateQuizResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateQuiz([FromBody] CreateQuizRequest request)
    {
        // Validate ModelState
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for quiz creation");
            return ValidationProblem(ModelState);
        }

        // Check authentication from COOKIE (shared between HTTP and SignalR)
        var userIdString = HttpContext.Request.Cookies[CookieKeys.UserId];
        if (string.IsNullOrEmpty(userIdString))
        {
            _logger.LogWarning("Unauthenticated user attempted to create quiz");
            return Unauthorized(new { error = "Not authenticated" });
        }

        if (!Guid.TryParse(userIdString, out var userId))
        {
            _logger.LogError("Invalid userId in cookie: {UserId}", userIdString);
            return Unauthorized(new { error = "Not authenticated" });
        }

        // Validate request data
        var validationErrors = ValidateQuizRequest(request);
        if (validationErrors.Any())
        {
            _logger.LogWarning("Quiz validation failed: {Errors}", string.Join(", ", validationErrors));
            return BadRequest(new { errors = validationErrors });
        }

        try
        {
            // Map request to DTO
            var questions = request.Questions.Select(q => new CreateQuizQuestionDto
            {
                QuestionText = q.QuestionText,
                QuestionOrder = q.QuestionOrder,
                Points = q.Points,
                Options = q.Options.Select(o => new CreateQuestionOptionDto
                {
                    OptionText = o.OptionText,
                    IsCorrect = o.IsCorrect,
                    OptionOrder = o.OptionOrder
                }).ToList()
            }).ToList();

            // Call gRPC client
            var result = await _quizClient.CreateQuizAsync(userId, request.Title, questions);

            if (result == null)
            {
                _logger.LogError("Failed to create quiz for user {UserId}", userId);
                return BadRequest(new { error = "Failed to create quiz" });
            }

            _logger.LogInformation("Quiz created successfully: {QuizId} by user {UserId}", result.QuizId, userId);
            return StatusCode(201, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating quiz for user {UserId}", userId);
            return StatusCode(500, new { error = "An error occurred while creating the quiz" });
        }
    }

    /// <summary>
    /// Deletes a quiz (only if user is the creator)
    /// </summary>
    /// <param name="quizId">ID of the quiz to delete</param>
    /// <returns>Deletion result</returns>
    /// <response code="200">Quiz deleted successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized to delete this quiz</response>
    /// <response code="404">Quiz not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{quizId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteQuiz(Guid quizId)
    {
        // Check authentication
        var userIdString = HttpContext.Session.GetString(SessionKeys.UserId);
        if (string.IsNullOrEmpty(userIdString))
        {
            _logger.LogWarning("Unauthenticated user attempted to delete quiz {QuizId}", quizId);
            return Unauthorized(new { error = "Not authenticated" });
        }

        if (!Guid.TryParse(userIdString, out var userId))
        {
            _logger.LogError("Invalid userId in session: {UserId}", userIdString);
            return Unauthorized(new { error = "Not authenticated" });
        }

        try
        {
            // Call gRPC client
            var (success, message) = await _quizClient.DeleteQuizAsync(userId, quizId);

            // Handle different failure scenarios
            if (!success)
            {
                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Quiz not found: {QuizId}", quizId);
                    return NotFound(new { success = false, message = "Quiz not found" });
                }

                if (message.Contains("own quizzes", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("User {UserId} attempted to delete quiz {QuizId} without authorization", userId, quizId);
                    return StatusCode(403, new { success = false, message = "You can only delete your own quizzes" });
                }

                _logger.LogWarning("Failed to delete quiz {QuizId}: {Message}", quizId, message);
                return BadRequest(new { success = false, message });
            }

            _logger.LogInformation("Quiz {QuizId} deleted successfully by user {UserId}", quizId, userId);
            return Ok(new { success = true, message = "Quiz deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting quiz {QuizId} for user {UserId}", quizId, userId);
            return StatusCode(500, new { error = "An error occurred while deleting the quiz" });
        }
    }
    
    // Add to QuizManagementController.cs
    /// <summary>
    /// Gets all quizzes available for the user to play (created by others)
    /// </summary>
    /// <returns>List of quiz summaries from other users</returns>
    /// <response code="200">Quizzes retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("available")]
    [ProducesResponseType(typeof(List<QuizSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAvailableQuizzes()
    {
        // Check authentication
        var userIdString = HttpContext.Session.GetString(SessionKeys.UserId);
        if (string.IsNullOrEmpty(userIdString))
        {
            _logger.LogWarning("Unauthenticated user attempted to retrieve available quizzes");
            return Unauthorized(new { error = "Not authenticated" });
        }

        if (!Guid.TryParse(userIdString, out var userId))
        {
            _logger.LogError("Invalid userId in session: {UserId}", userIdString);
            return Unauthorized(new { error = "Not authenticated" });
        }

        try
        {
            // Call gRPC client
            var quizzes = await _quizClient.GetAvailableQuizzesAsync(userId);

            _logger.LogInformation("Retrieved {Count} available quizzes for user {UserId}", quizzes.Count, userId);
            return Ok(quizzes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available quizzes for user {UserId}", userId);
            return StatusCode(500, new { error = "An error occurred while retrieving available quizzes" });
        }
    }

    /// <summary>
    /// Gets all quizzes created by the authenticated user
    /// </summary>
    /// <returns>List of quiz summaries</returns>
    /// <response code="200">Quizzes retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("myquizzes")]
    [ProducesResponseType(typeof(List<QuizSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMyQuizzes()
    {
        // Check authentication
        var userIdString = HttpContext.Session.GetString(SessionKeys.UserId);
        if (string.IsNullOrEmpty(userIdString))
        {
            _logger.LogWarning("Unauthenticated user attempted to retrieve quizzes");
            return Unauthorized(new { error = "Not authenticated" });
        }

        if (!Guid.TryParse(userIdString, out var userId))
        {
            _logger.LogError("Invalid userId in session: {UserId}", userIdString);
            return Unauthorized(new { error = "Not authenticated" });
        }

        try
        {
            // Call gRPC client
            var quizzes = await _quizClient.GetUserQuizzesAsync(userId);

            _logger.LogInformation("Retrieved {Count} quizzes for user {UserId}", quizzes.Count, userId);
            return Ok(quizzes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving quizzes for user {UserId}", userId);
            return StatusCode(500, new { error = "An error occurred while retrieving quizzes" });
        }
    }

    /// <summary>
    /// Gets full details of a specific quiz including all questions and options
    /// </summary>
    /// <param name="quizId">ID of the quiz to retrieve</param>
    /// <returns>Quiz details with questions and options</returns>
    /// <response code="200">Quiz retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized to view this quiz</response>
    /// <response code="404">Quiz not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{quizId}")]
    [ProducesResponseType(typeof(QuizDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetQuiz(Guid quizId)
    {
        // Check authentication from COOKIE (shared between HTTP and SignalR)
        var userIdString = HttpContext.Request.Cookies[CookieKeys.UserId];
        if (string.IsNullOrEmpty(userIdString))
        {
            _logger.LogWarning("Unauthenticated user attempted to retrieve quiz {QuizId}", quizId);
            return Unauthorized(new { error = "Not authenticated" });
        }

        if (!Guid.TryParse(userIdString, out var userId))
        {
            _logger.LogError("Invalid userId in session: {UserId}", userIdString);
            return Unauthorized(new { error = "Not authenticated" });
        }

        try
        {
            // Call gRPC client
            var quiz = await _quizClient.GetQuizAsync(quizId, userId);

            if (quiz == null)
            {
                _logger.LogWarning("Quiz not found or access denied: {QuizId} for user {UserId}", quizId, userId);
                return NotFound(new { error = "Quiz not found" });
            }

            _logger.LogInformation("Retrieved quiz {QuizId} for user {UserId}", quizId, userId);
            return Ok(quiz);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving quiz {QuizId} for user {UserId}", quizId, userId);
            return StatusCode(500, new { error = "An error occurred while retrieving the quiz" });
        }
    }

    /// <summary>
    /// Submits quiz answers and returns the graded result
    /// </summary>
    /// <param name="quizId">ID of the quiz</param>
    /// <param name="request">Submission request with answers</param>
    /// <returns>Quiz submission result with score</returns>
    /// <response code="200">Quiz submitted successfully</response>
    /// <response code="400">Invalid submission data</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Quiz not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{quizId}/submit")]
    [ProducesResponseType(typeof(QuizSubmissionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SubmitQuiz(Guid quizId, [FromBody] SubmitQuizRequest request)
    {
        // Validate ModelState
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for quiz submission");
            return ValidationProblem(ModelState);
        }

        // Check authentication from COOKIE
        var userIdString = HttpContext.Request.Cookies[CookieKeys.UserId];
        if (string.IsNullOrEmpty(userIdString))
        {
            _logger.LogWarning("Unauthenticated user attempted to submit quiz {QuizId}", quizId);
            return Unauthorized(new { error = "Not authenticated" });
        }

        if (!Guid.TryParse(userIdString, out var userId))
        {
            _logger.LogError("Invalid userId in cookie: {UserId}", userIdString);
            return Unauthorized(new { error = "Not authenticated" });
        }

        // Validate answers
        if (request.Answers == null || request.Answers.Count == 0)
        {
            _logger.LogWarning("Quiz submission with no answers for quiz {QuizId}", quizId);
            return BadRequest(new { error = "At least one answer is required" });
        }

        try
        {
            // Call gRPC client
            var result = await _quizClient.SubmitQuizAsync(quizId, userId, request.Answers);

            if (result == null)
            {
                _logger.LogError("Failed to submit quiz {QuizId} for user {UserId}", quizId, userId);
                return NotFound(new { error = "Quiz not found or submission failed" });
            }

            _logger.LogInformation(
                "Quiz {QuizId} submitted by user {UserId}: Score {Score}/{TotalPoints} ({Percentage:F1}%)",
                quizId, userId, result.Score, result.TotalPoints, result.Percentage);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting quiz {QuizId} for user {UserId}", quizId, userId);
            return StatusCode(500, new { error = "An error occurred while submitting the quiz" });
        }
    }

    /// <summary>
    /// Validates quiz request data
    /// </summary>
    /// <param name="request">Quiz request to validate</param>
    /// <returns>List of validation errors (empty if valid)</returns>
    private List<string> ValidateQuizRequest(CreateQuizRequest request)
    {
        var errors = new List<string>();

        // Validate title length (already done by DataAnnotations, but double-checking)
        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length < 3 || request.Title.Length > 200)
        {
            errors.Add("Title must be between 3 and 200 characters");
        }

        // Validate minimum questions
        if (request.Questions == null || request.Questions.Count < 1)
        {
            errors.Add("At least 1 question is required");
        }
        else
        {
            // Validate each question
            for (int i = 0; i < request.Questions.Count; i++)
            {
                var question = request.Questions[i];

                // Validate options count
                if (question.Options == null || question.Options.Count < 2)
                {
                    errors.Add($"Question {i + 1}: At least 2 options are required");
                }
                else if (question.Options.Count > 6)
                {
                    errors.Add($"Question {i + 1}: Maximum 6 options allowed");
                }
                else
                {
                    // Validate exactly one correct answer
                    var correctCount = question.Options.Count(o => o.IsCorrect);
                    if (correctCount != 1)
                    {
                        errors.Add($"Question {i + 1}: Exactly 1 option must be marked as correct (found {correctCount})");
                    }
                }
            }
        }

        return errors;
    }
}
