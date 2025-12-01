package com.semesterprojekt.quiz.service;

import com.semesterprojekt.quiz.*;
import com.semesterprojekt.quiz.exception.QuizNotFoundException;

import com.semesterprojekt.proto.quiz.CreateQuizRequest;
import com.semesterprojekt.proto.quiz.CreateQuizQuestion;
import com.semesterprojekt.proto.quiz.CreateQuestionOption;

import com.semesterprojekt.proto.quiz.*;


import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.UUID;

@Service
public class QuizService {

  private final QuizRepository quizRepository;

  public QuizService(QuizRepository quizRepository) {
    this.quizRepository = quizRepository;
  }

  /**
   * Creates a quiz from a gRPC CreateQuizRequest
   */
  @Transactional
  public QuizEntity createQuiz(CreateQuizRequest request) {

    QuizEntity quiz = new QuizEntity();
    quiz.setTitle(request.getTitle());
    quiz.setCreatedBy(UUID.fromString(request.getCreatedBy())); // <-- FIX

    // For each proto question → JPA Question entity
    for (CreateQuizQuestion qReq : request.getQuestionsList()) {

      Question q = new Question();
      q.setQuestionText(qReq.getQuestionText());
      q.setQuestionOrder(qReq.getQuestionOrder());
      q.setPoints(qReq.getPoints());
      q.setQuiz(quiz);  // relationship

      // Add options
      for (CreateQuestionOption optReq : qReq.getOptionsList()) {

        QuestionOptionEntity opt = new QuestionOptionEntity();
        opt.setOptionText(optReq.getOptionText());
        opt.setCorrect(optReq.getIsCorrect());
        opt.setOptionOrder(optReq.getOptionOrder());
        opt.setQuestion(q); // relationship

        q.getOptions().add(opt);
      }

      quiz.getQuestions().add(q);
    }

    return quizRepository.save(quiz);
  }

  /**
   * Get quiz by ID
   */
  @Transactional(readOnly = true)
  public QuizEntity getQuiz(UUID id) {
    return quizRepository.findById(id)
        .orElseThrow(() -> new QuizNotFoundException(id.toString()));
  }

  /**
   * Get quizzes for user
   */
  @Transactional(readOnly = true)
  public java.util.List<QuizEntity> getUserQuizzes(String userId) {
    return quizRepository.findByCreatedBy(UUID.fromString(userId));
  }

  /**
   * Calculate total points
   */
  public int calculateTotalPoints(QuizEntity quiz) {
    return quiz.getQuestions().stream()
        .mapToInt(Question::getPoints)
        .sum();
  }

  /**
   * Delete quiz
   */
  @Transactional
  public boolean deleteQuiz(UUID quizId, String userId) {
    QuizEntity quiz = getQuiz(quizId);

    // Only creator can delete
    if (!quiz.getCreatedBy().equals(userId)) {
      return false;
    }

    quizRepository.delete(quiz);
    return true;
  }

  @Transactional
  public SubmitQuizResponse gradeQuiz(SubmitQuizRequest request) {

    UUID quizId = UUID.fromString(request.getQuizId());
    QuizEntity quiz = getQuiz(quizId);

    SubmitQuizResponse.Builder response = SubmitQuizResponse.newBuilder();

    int totalPoints = calculateTotalPoints(quiz);
    int score = 0;

    // Add result details
    SubmitQuizResponse.Builder res = SubmitQuizResponse.newBuilder();

    // Build list of result entries
    for (QuizAnswer answer : request.getAnswersList()) {

      String questionId = answer.getQuestionId();
      String selectedOptionId = answer.getSelectedOptionId();

      Question question =
          quiz.getQuestions().stream()
              .filter(q -> q.getId().toString().equals(questionId))
              .findFirst()
              .orElse(null);

      // If question not found, skip (should not happen)
      if (question == null) continue;

      // Find the chosen option
      QuestionOptionEntity chosen =
          question.getOptions().stream()
              .filter(o -> o.getId().toString().equals(selectedOptionId))
              .findFirst()
              .orElse(null);

      boolean isCorrect = chosen != null && chosen.isCorrect();
      int pointsEarned = isCorrect ? question.getPoints() : 0;

      score += pointsEarned;

      // Build AnswerResult
      AnswerResult result = AnswerResult.newBuilder()
          .setQuestionId(questionId)
          .setIsCorrect(isCorrect)
          .setPointsEarned(pointsEarned)
          .build();

      res.addResults(result);
    }

    // Build final response
    response.setSuccess(true);
    response.setScore(score);
    response.setTotalPoints(totalPoints);
    response.addAllResults(res.getResultsList());

    return response.build();
  }
}
