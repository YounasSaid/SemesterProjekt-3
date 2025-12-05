using Grpc.Net.Client;
using SemesterProjekt.Proto.Quiz;
using AppServer.Models;

namespace AppServer.Services;

/// <summary>
/// gRPC client for quiz-related operations
/// </summary>
public class QuizGrpcClient : IDisposable
{
    private readonly GrpcChannel _channel;
    private readonly QuizService.QuizServiceClient _quizClient;
    private readonly ILogger<QuizGrpcClient> _logger;

    public QuizGrpcClient(IConfiguration config, ILogger<QuizGrpcClient> logger)
    {
        _logger = logger;
        var grpcAddress = config["GrpcSettings:DataServerAddress"] 
                         ?? "http://localhost:9090";
        
        _channel = GrpcChannel.ForAddress(grpcAddress);
        _quizClient = new QuizService.QuizServiceClient(_channel);
    }

    /// <summary>
    /// Creates a new quiz with questions and options
    /// </summary>
    public async Task<CreateQuizResultDto?> CreateQuizAsync(
        Guid userId, 
        string title, 
        List<CreateQuizQuestionDto> questions)
    {
        try
        {
            var userIdString = userId.ToString();

            var request = new SemesterProjekt.Proto.Quiz.CreateQuizRequest
            {
                CreatedBy = userIdString,
                Title = title
            };

            foreach (var question in questions)
            {
                var protoQuestion = new SemesterProjekt.Proto.Quiz.CreateQuizQuestion
                {
                    QuestionText = question.QuestionText,
                    QuestionOrder = question.QuestionOrder,
                    Points = question.Points
                };

                foreach (var option in question.Options)
                {
                    protoQuestion.Options.Add(new SemesterProjekt.Proto.Quiz.CreateQuestionOption
                    {
                        OptionText = option.OptionText,
                        IsCorrect = option.IsCorrect,
                        OptionOrder = option.OptionOrder
                    });
                }

                request.Questions.Add(protoQuestion);
            }

            var response = await _quizClient.CreateQuizAsync(request);

            if (!response.Success)
            {
                _logger.LogError("Failed to create quiz: {ErrorMessage}", response.ErrorMessage);
                return null;
            }

            return new CreateQuizResultDto
            {
                QuizId = Guid.Parse(response.QuizId),
                Title = response.Title,
                QuestionCount = response.QuestionCount,
                TotalPoints = response.TotalPoints
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating quiz");
            return null;
        }
    }

    /// <summary>
    /// Deletes a quiz (only if the user is the creator)
    /// </summary>
    public async Task<(bool success, string message)> DeleteQuizAsync(Guid userId, Guid quizId)
    {
        try
        {
            var request = new SemesterProjekt.Proto.Quiz.DeleteQuizRequest
            {
                UserId = userId.ToString(),
                QuizId = quizId.ToString()
            };

            var response = await _quizClient.DeleteQuizAsync(request);
            return (response.Success, response.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting quiz");
            return (false, "Error deleting quiz");
        }
    }
    
    // Add to QuizGrpcClient.cs
    /// <summary>
    /// Gets all quizzes NOT created by the specified user (available to play)
    /// </summary>
    /// <param name="userId">ID of the user</param>
    /// <returns>List of quiz summaries from other users</returns>
    public async Task<List<QuizSummaryDto>> GetAvailableQuizzesAsync(Guid userId)
    {
        try
        {
            // Convert userId to string
            var request = new SemesterProjekt.Proto.Quiz.GetAvailableQuizzesRequest
            {
                UserId = userId.ToString()
            };

            // Call gRPC service
            var response = await _quizClient.GetAvailableQuizzesAsync(request);

            // Map response to DTOs
            var quizzes = new List<QuizSummaryDto>();
            foreach (var quiz in response.Quizzes)
            {
                quizzes.Add(new QuizSummaryDto
                {
                    QuizId = Guid.Parse(quiz.QuizId),
                    Title = quiz.Title,
                    QuestionCount = quiz.QuestionCount,
                    TotalPoints = quiz.TotalPoints,
                    CreatedAt = DateTime.Parse(quiz.CreatedAt)
                });
            }

            return quizzes;
        }
        catch (Grpc.Core.RpcException ex)
        {
            _logger.LogError(ex, "gRPC error when getting available quizzes");
            return new List<QuizSummaryDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when getting available quizzes");
            return new List<QuizSummaryDto>();
        }
    }

    /// <summary>
    /// Gets all quizzes created by a specific user
    /// </summary>
    public async Task<List<QuizSummaryDto>> GetUserQuizzesAsync(Guid userId)
    {
        try
        {
            var request = new SemesterProjekt.Proto.Quiz.GetUserQuizzesRequest
            {
                UserId = userId.ToString()
            };

            var response = await _quizClient.GetUserQuizzesAsync(request);

            var quizzes = new List<QuizSummaryDto>();
            foreach (var quiz in response.Quizzes)
            {
                quizzes.Add(new QuizSummaryDto
                {
                    QuizId = Guid.Parse(quiz.QuizId),
                    Title = quiz.Title,
                    QuestionCount = quiz.QuestionCount,
                    TotalPoints = quiz.TotalPoints,
                    CreatedAt = DateTime.Parse(quiz.CreatedAt)
                });
            }

            return quizzes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user quizzes");
            return new List<QuizSummaryDto>();
        }
    }

    /// <summary>
    /// Gets full details of a specific quiz with all questions and options
    /// </summary>
    public async Task<QuizDetailsDto?> GetQuizAsync(Guid quizId, Guid userId)
    {
        try
        {
            var request = new SemesterProjekt.Proto.Quiz.GetQuizRequest
            {
                QuizId = quizId.ToString(),
                UserId = userId.ToString()
            };

            var response = await _quizClient.GetQuizAsync(request);

            if (response == null || !response.Found)
            {
                return null;
            }

            var quiz = new QuizDetailsDto
            {
                QuizId = Guid.Parse(response.QuizId),
                Title = response.Title,
                CreatedAt = DateTime.Parse(response.CreatedAt),
                TotalPoints = response.TotalPoints,
                Questions = new List<QuestionDto>()
            };

            foreach (var q in response.Questions)
            {
                var question = new QuestionDto
                {
                    QuestionId = Guid.Parse(q.QuestionId),
                    QuestionText = q.QuestionText,
                    QuestionOrder = q.QuestionOrder,
                    Points = q.Points,
                    Options = new List<QuestionOptionDto>()
                };

                foreach (var o in q.Options)
                {
                    question.Options.Add(new QuestionOptionDto
                    {
                        OptionId = Guid.Parse(o.OptionId),
                        OptionText = o.OptionText,
                        OptionOrder = o.OptionOrder,
                        IsCorrect = o.IsCorrect
                    });
                }

                quiz.Questions.Add(question);
            }

            return quiz;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quiz {QuizId}", quizId);
            return null;
        }
    }

    /// <summary>
    /// Submits quiz answers and returns the graded result (with duration)
    /// </summary>
    public async Task<QuizSubmissionResultDto?> SubmitQuizAsync(
        Guid quizId, 
        Guid userId, 
        List<QuizAnswerDto> answers,
        int durationSeconds = 0)
    {
        try
        {
            var request = new SemesterProjekt.Proto.Quiz.SubmitQuizRequest
            {
                QuizId = quizId.ToString(),
                UserId = userId.ToString(),
                DurationSeconds = durationSeconds
            };

            foreach (var answer in answers)
            {
                request.Answers.Add(new SemesterProjekt.Proto.Quiz.QuizAnswer
                {
                    QuestionId = answer.QuestionId.ToString(),
                    SelectedOptionId = answer.SelectedOptionId.ToString()
                });
            }

            var response = await _quizClient.SubmitQuizAsync(request);

            if (!response.Success)
            {
                _logger.LogError("Failed to submit quiz {QuizId}", quizId);
                return null;
            }

            var result = new QuizSubmissionResultDto
            {
                Success = response.Success,
                Score = response.Score,
                TotalPoints = response.TotalPoints,
                Percentage = response.TotalPoints > 0 
                    ? (double)response.Score / response.TotalPoints * 100 
                    : 0,
                AttemptId = response.AttemptId,
                IsNewBest = response.IsNewBest,
                PreviousBestScore = response.PreviousBestScore,
                Results = new List<AnswerResultDto>()
            };

            foreach (var r in response.Results)
            {
                result.Results.Add(new AnswerResultDto
                {
                    QuestionId = Guid.Parse(r.QuestionId),
                    IsCorrect = r.IsCorrect,
                    PointsEarned = r.PointsEarned
                });
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting quiz {QuizId}", quizId);
            return null;
        }
    }

    /// <summary>
    /// Gets user statistics (total score, quizzes taken, etc.)
    /// </summary>
    public async Task<UserStatsDto?> GetUserStatsAsync(Guid userId)
    {
        try
        {
            var request = new GetUserStatsRequest
            {
                UserId = userId.ToString()
            };

            var response = await _quizClient.GetUserStatsAsync(request);

            var stats = new UserStatsDto
            {
                TotalScore = response.TotalScore,
                QuizzesTaken = response.QuizzesTaken,
                TotalAttempts = response.TotalAttempts,
                QuizzesCreated = response.QuizzesCreated,
                AveragePercentage = response.AveragePercentage,
                RecentAttempts = new List<AttemptSummaryDto>()
            };

            foreach (var attempt in response.RecentAttempts)
            {
                stats.RecentAttempts.Add(MapAttemptSummary(attempt));
            }

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user stats");
            return null;
        }
    }

    /// <summary>
    /// Gets all attempts for a user
    /// </summary>
    public async Task<List<AttemptSummaryDto>> GetUserAttemptsAsync(Guid userId, int limit = 20)
    {
        try
        {
            var request = new GetUserAttemptsRequest
            {
                UserId = userId.ToString(),
                Limit = limit
            };

            var response = await _quizClient.GetUserAttemptsAsync(request);

            var attempts = new List<AttemptSummaryDto>();
            foreach (var attempt in response.Attempts)
            {
                attempts.Add(MapAttemptSummary(attempt));
            }

            return attempts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user attempts");
            return new List<AttemptSummaryDto>();
        }
    }

    private AttemptSummaryDto MapAttemptSummary(AttemptSummary attempt)
    {
        return new AttemptSummaryDto
        {
            AttemptId = Guid.Parse(attempt.AttemptId),
            QuizId = Guid.Parse(attempt.QuizId),
            QuizTitle = attempt.QuizTitle,
            Score = attempt.Score,
            TotalPoints = attempt.TotalPoints,
            CorrectCount = attempt.CorrectCount,
            TotalCount = attempt.TotalCount,
            DurationSeconds = attempt.DurationSeconds,
            CompletedAt = DateTime.Parse(attempt.CompletedAt),
            IsBestAttempt = attempt.IsBestAttempt,
            Percentage = attempt.Percentage
        };
    }

    public void Dispose()
    {
        _channel?.Dispose();
    }
}
