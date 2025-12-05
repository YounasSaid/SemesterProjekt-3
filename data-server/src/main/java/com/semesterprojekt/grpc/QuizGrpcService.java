package com.semesterprojekt.grpc;

import com.semesterprojekt.quiz.*;
import com.semesterprojekt.quiz.service.QuizService;
import com.semesterprojekt.user.User;
import com.semesterprojekt.user.UserRepository;
import com.semesterprojekt.proto.quiz.*;

import io.grpc.Status;
import io.grpc.stub.StreamObserver;
import net.devh.boot.grpc.server.service.GrpcService;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

/**
 * Class: QuizGrpcService
 * --------------------------------------------
 * Purpose:
 *   Implements the gRPC API for quiz operations.
 */
@GrpcService
public class QuizGrpcService extends QuizServiceGrpc.QuizServiceImplBase {

  private final QuizService quizService;
  private final QuizRepository quizRepository;
  private final UserRepository userRepository;

  public QuizGrpcService(QuizService quizService, 
                         QuizRepository quizRepository,
                         UserRepository userRepository) {
    this.quizService = quizService;
    this.quizRepository = quizRepository;
    this.userRepository = userRepository;
  }

  // ----------------------
  // Create Quiz
  // ----------------------
  @Override
  public void createQuiz(CreateQuizRequest request,
      StreamObserver<CreateQuizResponse> responseObserver) {

    try {
      QuizEntity quiz = quizService.createQuiz(request);

      CreateQuizResponse response = CreateQuizResponse.newBuilder()
          .setSuccess(true)
          .setQuizId(quiz.getId().toString())
          .setTitle(quiz.getTitle())
          .setQuestionCount(quiz.getQuestions().size())
          .setTotalPoints(quizService.calculateTotalPoints(quiz))
          .build();

      responseObserver.onNext(response);
      responseObserver.onCompleted();

    } catch (Exception ex) {
      responseObserver.onError(
          Status.INTERNAL.withDescription("Failed to create quiz: " + ex.getMessage())
              .asRuntimeException()
      );
    }
  }

  // ----------------------
  // Delete Quiz
  // ----------------------
  @Override
  public void deleteQuiz(DeleteQuizRequest request,
      StreamObserver<DeleteQuizResponse> responseObserver) {

    try {
      boolean deleted = quizService.deleteQuiz(
          UUID.fromString(request.getQuizId()),
          request.getUserId()
      );

      DeleteQuizResponse resp = DeleteQuizResponse.newBuilder()
          .setSuccess(deleted)
          .setMessage(deleted ? "Quiz deleted" : "Quiz not deleted")
          .build();

      responseObserver.onNext(resp);
      responseObserver.onCompleted();

    } catch (Exception ex) {
      responseObserver.onError(
          Status.INTERNAL.withDescription("Failed to delete quiz: " + ex.getMessage())
              .asRuntimeException()
      );
    }
  }

  // ----------------------
  // Get User Quizzes
  // ----------------------
  @Override
  public void getUserQuizzes(GetUserQuizzesRequest request,
      StreamObserver<GetUserQuizzesResponse> responseObserver) {

    try {
      var quizzes = quizService.getUserQuizzes(request.getUserId());

      GetUserQuizzesResponse.Builder builder = GetUserQuizzesResponse.newBuilder();

      quizzes.forEach(q -> builder.addQuizzes(
          QuizSummary.newBuilder()
              .setQuizId(q.getId().toString())
              .setTitle(q.getTitle())
              .setQuestionCount(q.getQuestions().size())
              .setTotalPoints(quizService.calculateTotalPoints(q))
              .setCreatedAt(q.getCreatedAt().toString())
      ));

      responseObserver.onNext(builder.build());
      responseObserver.onCompleted();

    } catch (Exception ex) {
      responseObserver.onError(
          Status.INTERNAL.withDescription("Failed to get user quizzes: " + ex.getMessage())
              .asRuntimeException()
      );
    }
  }

  // ----------------------
  // Get Quiz (play quiz)
  // ----------------------
  @Override
  public void getQuiz(GetQuizRequest request,
      StreamObserver<GetQuizResponse> responseObserver) {

    try {
      UUID quizId = UUID.fromString(request.getQuizId());
      QuizEntity quiz = quizService.getQuiz(quizId);

      if (quiz == null) {
        responseObserver.onNext(
            GetQuizResponse.newBuilder()
                .setFound(false)
                .build()
        );
        responseObserver.onCompleted();
        return;
      }

      GetQuizResponse.Builder resp = GetQuizResponse.newBuilder()
          .setFound(true)
          .setQuizId(quiz.getId().toString())
          .setTitle(quiz.getTitle())
          .setCreatedBy(quiz.getCreatedBy().toString())
          .setCreatedAt(quiz.getCreatedAt().toString())
          .setTotalPoints(quizService.calculateTotalPoints(quiz));

      for (Question q : quiz.getQuestions()) {
        QuizQuestion.Builder qBuilder = QuizQuestion.newBuilder()
            .setQuestionId(q.getId().toString())
            .setQuestionText(q.getQuestionText())
            .setQuestionOrder(q.getQuestionOrder())
            .setPoints(q.getPoints());

        for (QuestionOptionEntity opt : q.getOptions()) {
          qBuilder.addOptions(
              QuestionOption.newBuilder()
                  .setOptionId(opt.getId().toString())
                  .setOptionText(opt.getOptionText())
                  .setOptionOrder(opt.getOptionOrder())
                  .setIsCorrect(opt.isCorrect())
          );
        }

        resp.addQuestions(qBuilder);
      }

      responseObserver.onNext(resp.build());
      responseObserver.onCompleted();

    } catch (Exception ex) {
      responseObserver.onError(
          Status.INTERNAL.withDescription("Failed to get quiz: " + ex.getMessage())
              .asRuntimeException()
      );
    }
  }

  // ----------------------
  // Submit Quiz (grade AND save attempt)
  // ----------------------
  @Override
  public void submitQuiz(SubmitQuizRequest request,
      StreamObserver<SubmitQuizResponse> responseObserver) {

    try {
      SubmitQuizResponse response = quizService.gradeAndSaveQuiz(request);

      responseObserver.onNext(response);
      responseObserver.onCompleted();

    } catch (Exception ex) {
      responseObserver.onError(
          Status.INTERNAL.withDescription("Failed to submit quiz: " + ex.getMessage())
              .asRuntimeException()
      );
    }
  }

  // ----------------------
  // Get User Attempts
  // ----------------------
  @Override
  public void getUserAttempts(GetUserAttemptsRequest request,
      StreamObserver<GetUserAttemptsResponse> responseObserver) {

    try {
      UUID userId = UUID.fromString(request.getUserId());
      int limit = request.getLimit() > 0 ? request.getLimit() : 20;
      
      List<QuizAttempt> attempts = quizService.getUserAttempts(userId, limit);

      GetUserAttemptsResponse.Builder builder = GetUserAttemptsResponse.newBuilder();

      for (QuizAttempt attempt : attempts) {
        // Get quiz title
        String quizTitle = quizRepository.findById(attempt.getQuizId())
            .map(QuizEntity::getTitle)
            .orElse("Unknown Quiz");

        builder.addAttempts(buildAttemptSummary(attempt, quizTitle));
      }

      responseObserver.onNext(builder.build());
      responseObserver.onCompleted();

    } catch (Exception ex) {
      responseObserver.onError(
          Status.INTERNAL.withDescription("Failed to get user attempts: " + ex.getMessage())
              .asRuntimeException()
      );
    }
  }

  // ----------------------
  // Get Quiz Attempts
  // ----------------------
  @Override
  public void getQuizAttempts(GetQuizAttemptsRequest request,
      StreamObserver<GetQuizAttemptsResponse> responseObserver) {

    try {
      UUID userId = UUID.fromString(request.getUserId());
      UUID quizId = UUID.fromString(request.getQuizId());

      List<QuizAttempt> attempts = quizService.getQuizAttempts(userId, quizId);
      Optional<QuizAttempt> bestAttempt = quizService.getBestAttempt(userId, quizId);

      String quizTitle = quizRepository.findById(quizId)
          .map(QuizEntity::getTitle)
          .orElse("Unknown Quiz");

      GetQuizAttemptsResponse.Builder builder = GetQuizAttemptsResponse.newBuilder()
          .setAttemptCount(attempts.size());

      for (QuizAttempt attempt : attempts) {
        builder.addAttempts(buildAttemptSummary(attempt, quizTitle));
      }

      bestAttempt.ifPresent(attempt -> 
          builder.setBestAttempt(buildAttemptSummary(attempt, quizTitle))
      );

      responseObserver.onNext(builder.build());
      responseObserver.onCompleted();

    } catch (Exception ex) {
      responseObserver.onError(
          Status.INTERNAL.withDescription("Failed to get quiz attempts: " + ex.getMessage())
              .asRuntimeException()
      );
    }
  }

  // ----------------------
  // Get User Stats
  // ----------------------
  @Override
  public void getUserStats(GetUserStatsRequest request,
      StreamObserver<GetUserStatsResponse> responseObserver) {

    try {
      UUID userId = UUID.fromString(request.getUserId());

      // Get user for total score
      User user = userRepository.findById(userId).orElse(null);
      int totalScore = user != null ? user.getTotalScore() : 0;

      // Get statistics
      long quizzesTaken = quizService.countQuizzesTaken(userId);
      long totalAttempts = quizService.countTotalAttempts(userId);
      long quizzesCreated = quizService.countQuizzesCreated(userId);

      // Calculate average percentage from best attempts
      List<QuizAttempt> recentAttempts = quizService.getUserAttempts(userId, 5);
      double averagePercentage = recentAttempts.stream()
          .filter(QuizAttempt::isBestAttempt)
          .mapToDouble(QuizAttempt::getPercentage)
          .average()
          .orElse(0.0);

      GetUserStatsResponse.Builder builder = GetUserStatsResponse.newBuilder()
          .setTotalScore(totalScore)
          .setQuizzesTaken((int) quizzesTaken)
          .setTotalAttempts((int) totalAttempts)
          .setQuizzesCreated((int) quizzesCreated)
          .setAveragePercentage(averagePercentage);

      // Add recent attempts
      for (QuizAttempt attempt : recentAttempts) {
        String quizTitle = quizRepository.findById(attempt.getQuizId())
            .map(QuizEntity::getTitle)
            .orElse("Unknown Quiz");
        builder.addRecentAttempts(buildAttemptSummary(attempt, quizTitle));
      }

      responseObserver.onNext(builder.build());
      responseObserver.onCompleted();

    } catch (Exception ex) {
      responseObserver.onError(
          Status.INTERNAL.withDescription("Failed to get user stats: " + ex.getMessage())
              .asRuntimeException()
      );
    }
  }

  // Add to QuizGrpcService.java
  @Override
  public void getAvailableQuizzes(GetAvailableQuizzesRequest request,
                                  StreamObserver<GetAvailableQuizzesResponse> responseObserver) {

    try {
      var quizzes = quizService.getAvailableQuizzes(request.getUserId());

      GetAvailableQuizzesResponse.Builder builder = GetAvailableQuizzesResponse.newBuilder();

      quizzes.forEach(q -> builder.addQuizzes(
              QuizSummary.newBuilder()
                      .setQuizId(q.getId().toString())
                      .setTitle(q.getTitle())
                      .setQuestionCount(q.getQuestions().size())
                      .setTotalPoints(quizService.calculateTotalPoints(q))
                      .setCreatedAt(q.getCreatedAt().toString())
      ));

      responseObserver.onNext(builder.build());
      responseObserver.onCompleted();

    } catch (Exception ex) {
      responseObserver.onError(
              Status.INTERNAL.withDescription("Failed to get available quizzes: " + ex.getMessage())
                      .asRuntimeException()
      );
    }
  }

  // ----------------------
  // Helper method
  // ----------------------
  private AttemptSummary buildAttemptSummary(QuizAttempt attempt, String quizTitle) {
    return AttemptSummary.newBuilder()
        .setAttemptId(attempt.getId().toString())
        .setQuizId(attempt.getQuizId().toString())
        .setQuizTitle(quizTitle)
        .setScore(attempt.getScore())
        .setTotalPoints(attempt.getTotalPoints())
        .setCorrectCount(attempt.getCorrectCount())
        .setTotalCount(attempt.getTotalCount())
        .setDurationSeconds(attempt.getDurationSeconds())
        .setCompletedAt(attempt.getCompletedAt().toString())
        .setIsBestAttempt(attempt.isBestAttempt())
        .setPercentage(attempt.getPercentage())
        .build();
  }
}


