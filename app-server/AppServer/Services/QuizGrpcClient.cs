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
    /// <param name="userId">ID of the user creating the quiz</param>
    /// <param name="title">Title of the quiz</param>
    /// <param name="questions">List of questions with options</param>
    /// <returns>CreateQuizResultDto if successful, null otherwise</returns>
    public async Task<CreateQuizResultDto?> CreateQuizAsync(
        Guid userId, 
        string title, 
        List<CreateQuizQuestionDto> questions)
    {
        try
        {
            // Convert userId to string
            var userIdString = userId.ToString();

            // Build the CreateQuizRequest (PROTOBUF version)
            var request = new SemesterProjekt.Proto.Quiz.CreateQuizRequest
            {
                CreatedBy = userIdString,
                Title = title
            };

            // Map questions to protobuf messages
            foreach (var question in questions)
            {
                var protoQuestion = new SemesterProjekt.Proto.Quiz.CreateQuizQuestion
                {
                    QuestionText = question.QuestionText,
                    QuestionOrder = question.QuestionOrder,
                    Points = question.Points
                };

                // Map options to protobuf messages
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

            // Call gRPC service
            var response = await _quizClient.CreateQuizAsync(request);

            // Check for errors
            if (!response.Success)
            {
                _logger.LogError("Failed to create quiz: {ErrorMessage}", response.ErrorMessage);
                return null;
            }

            // Map response to DTO
            return new CreateQuizResultDto
            {
                QuizId = Guid.Parse(response.QuizId),
                Title = response.Title,
                QuestionCount = response.QuestionCount,
                TotalPoints = response.TotalPoints
            };
        }
        catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.InvalidArgument)
        {
            _logger.LogError(ex, "Invalid argument when creating quiz");
            return null;
        }
        catch (Grpc.Core.RpcException ex)
        {
            _logger.LogError(ex, "gRPC error when creating quiz");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when creating quiz");
            return null;
        }
    }

    /// <summary>
    /// Deletes a quiz (only if the user is the creator)
    /// </summary>
    /// <param name="userId">ID of the user attempting to delete</param>
    /// <param name="quizId">ID of the quiz to delete</param>
    /// <returns>Tuple with success status and message</returns>
    public async Task<(bool success, string message)> DeleteQuizAsync(Guid userId, Guid quizId)
    {
        try
        {
            // Convert GUIDs to strings
            var request = new SemesterProjekt.Proto.Quiz.DeleteQuizRequest
            {
                UserId = userId.ToString(),
                QuizId = quizId.ToString()
            };

            // Call gRPC service
            var response = await _quizClient.DeleteQuizAsync(request);

            return (response.Success, response.Message);
        }
        catch (Grpc.Core.RpcException ex)
        {
            _logger.LogError(ex, "gRPC error when deleting quiz");
            return (false, "Error deleting quiz");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when deleting quiz");
            return (false, "Error deleting quiz");
        }
    }

    /// <summary>
    /// Gets all quizzes created by a specific user
    /// </summary>
    /// <param name="userId">ID of the user</param>
    /// <returns>List of quiz summaries</returns>
    public async Task<List<QuizSummaryDto>> GetUserQuizzesAsync(Guid userId)
    {
        try
        {
            // Convert userId to string
            var request = new SemesterProjekt.Proto.Quiz.GetUserQuizzesRequest
            {
                UserId = userId.ToString()
            };

            // Call gRPC service
            var response = await _quizClient.GetUserQuizzesAsync(request);

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
            _logger.LogError(ex, "gRPC error when getting user quizzes");
            return new List<QuizSummaryDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when getting user quizzes");
            return new List<QuizSummaryDto>();
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
    }
}
