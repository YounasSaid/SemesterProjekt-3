package com.semesterprojekt.grpc;

import com.semesterprojekt.quiz.QuizEntity;
import com.semesterprojekt.quiz.Question;
import com.semesterprojekt.quiz.QuestionOptionEntity;
import com.semesterprojekt.quiz.service.QuizService;

import com.semesterprojekt.proto.quiz.QuestionOption;
import com.semesterprojekt.proto.quiz.SubmitQuizResponse;

import com.semesterprojekt.proto.quiz.*;

import io.grpc.Status;
import io.grpc.stub.StreamObserver;
import net.devh.boot.grpc.server.service.GrpcService;

import java.util.UUID;

/**
 * Class: QuizGrpcService
 * --------------------------------------------
 * Purpose:
 *   Implements the gRPC API for quiz operations.
 *
 * Responsibilities:
 *   - Receive requests from App server (C#)
 *   - Call QuizService to execute business logic
 *   - Convert domain objects → gRPC responses
 *   - Return success or error status codes
 */
@GrpcService
public class QuizGrpcService extends QuizServiceGrpc.QuizServiceImplBase {

  private final QuizService quizService;

  public QuizGrpcService(QuizService quizService) {
    this.quizService = quizService;
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
      QuizEntity quiz = quizService.getQuiz(UUID.fromString(request.getQuizId()));

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
          .setCreatedBy(quiz.getCreatedBy());

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
  // Submit Quiz
  // ----------------------
  @Override
  public void submitQuiz(SubmitQuizRequest request,
      StreamObserver<SubmitQuizResponse> responseObserver) {

    try {
      SubmitQuizResponse response = quizService.gradeQuiz(request);

      responseObserver.onNext(response);
      responseObserver.onCompleted();

    } catch (Exception ex) {
      responseObserver.onError(
          Status.INTERNAL.withDescription("Failed to submit quiz: " + ex.getMessage())
              .asRuntimeException()
      );
    }
  }
}
