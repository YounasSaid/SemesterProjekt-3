package com.semesterprojekt.quiz.service;

import com.semesterprojekt.quiz.*;
import com.semesterprojekt.user.User;
import com.semesterprojekt.user.UserRepository;
import com.semesterprojekt.proto.quiz.*;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.time.OffsetDateTime;
import java.util.ArrayList;
import java.util.List;
import java.util.Optional;
import java.util.UUID;

@Service
public class QuizService {

  private final QuizRepository quizRepository;
  private final QuizAttemptRepository attemptRepository;
  private final UserRepository userRepository;

  public QuizService(QuizRepository quizRepository, 
                     QuizAttemptRepository attemptRepository,
                     UserRepository userRepository) {
    this.quizRepository = quizRepository;
    this.attemptRepository = attemptRepository;
    this.userRepository = userRepository;
  }

  @Transactional
  public QuizEntity createQuiz(CreateQuizRequest request) {
    QuizEntity quiz = new QuizEntity();
    quiz.setCreatedBy(UUID.fromString(request.getCreatedBy()));
    quiz.setTitle(request.getTitle());

    List<Question> questions = new ArrayList<>();

    for (int i = 0; i < request.getQuestionsCount(); i++) {
      var reqQ = request.getQuestions(i);

      Question q = new Question();
      q.setQuestionText(reqQ.getQuestionText());
      q.setQuestionOrder(reqQ.getQuestionOrder());
      q.setPoints(reqQ.getPoints());
      q.setQuiz(quiz);

      List<QuestionOptionEntity> options = new ArrayList<>();
      for (int j = 0; j < reqQ.getOptionsCount(); j++) {
        var reqOpt = reqQ.getOptions(j);

        QuestionOptionEntity opt = new QuestionOptionEntity();
        opt.setOptionText(reqOpt.getOptionText());
        opt.setCorrect(reqOpt.getIsCorrect());
        opt.setOptionOrder(reqOpt.getOptionOrder());
        opt.setQuestion(q);

        options.add(opt);
      }
      q.setOptions(options);
      questions.add(q);
    }

    quiz.setQuestions(questions);
    return quizRepository.save(quiz);
  }

  @Transactional
  public boolean deleteQuiz(UUID quizId, String userIdStr) {
    UUID userId = UUID.fromString(userIdStr);
    return quizRepository.findById(quizId)
            .filter(quiz -> quiz.getCreatedBy().equals(userId))
            .map(quiz -> {
              quizRepository.delete(quiz);
              return true;
            })
            .orElse(false);
  }

  @Transactional(readOnly = true)
  public List<QuizEntity> getUserQuizzes(String userIdStr) {
    UUID userId = UUID.fromString(userIdStr);
    return quizRepository.findByCreatedByWithQuestions(userId);
  }

  @Transactional(readOnly = true)
  public QuizEntity getQuiz(UUID id) {
    return quizRepository.findById(id).orElse(null);
  }

  // Add to QuizService.java
  @Transactional(readOnly = true)
  public List<QuizEntity> getAvailableQuizzes(String userIdStr) {
    UUID userId = UUID.fromString(userIdStr);
    return quizRepository.findByCreatedByNotWithQuestions(userId);
  }

  public int calculateTotalPoints(QuizEntity quiz) {
    return quiz.getQuestions().stream()
            .mapToInt(Question::getPoints)
            .sum();
  }

  /**
   * Grade quiz AND save attempt to database
   */
  @Transactional
  public SubmitQuizResponse gradeAndSaveQuiz(SubmitQuizRequest request) {
    UUID quizId = UUID.fromString(request.getQuizId());
    UUID userId = UUID.fromString(request.getUserId());
    
    QuizEntity quiz = quizRepository.findById(quizId).orElse(null);

    if (quiz == null) {
      return SubmitQuizResponse.newBuilder()
              .setSuccess(false)
              .setScore(0)
              .setTotalPoints(0)
              .build();
    }

    int score = 0;
    int correctCount = 0;
    int totalPoints = calculateTotalPoints(quiz);
    int totalCount = quiz.getQuestions().size();
    
    List<AnswerResult> results = new ArrayList<>();
    List<AttemptAnswer> attemptAnswers = new ArrayList<>();

    // Grade each answer
    for (var answer : request.getAnswersList()) {
      UUID questionId = UUID.fromString(answer.getQuestionId());
      UUID selectedOptionId = UUID.fromString(answer.getSelectedOptionId());

      Question question = quiz.getQuestions().stream()
              .filter(q -> q.getId().equals(questionId))
              .findFirst()
              .orElse(null);

      if (question != null) {
        QuestionOptionEntity selectedOption = question.getOptions().stream()
                .filter(opt -> opt.getId().equals(selectedOptionId))
                .findFirst()
                .orElse(null);

        boolean isCorrect = selectedOption != null && selectedOption.isCorrect();
        int pointsEarned = isCorrect ? question.getPoints() : 0;
        score += pointsEarned;
        if (isCorrect) correctCount++;

        // Build gRPC result
        results.add(AnswerResult.newBuilder()
                .setQuestionId(questionId.toString())
                .setIsCorrect(isCorrect)
                .setPointsEarned(pointsEarned)
                .build());

        // Build attempt answer entity
        AttemptAnswer attemptAnswer = new AttemptAnswer();
        attemptAnswer.setQuestionId(questionId);
        attemptAnswer.setSelectedOptionId(selectedOptionId);
        attemptAnswer.setCorrect(isCorrect);
        attemptAnswer.setPointsEarned(pointsEarned);
        attemptAnswers.add(attemptAnswer);
      }
    }

    // Check if this is a new best score
    Optional<QuizAttempt> previousBest = attemptRepository.findBestAttempt(userId, quizId);
    int previousBestScore = previousBest.map(QuizAttempt::getScore).orElse(0);
    boolean isNewBest = score > previousBestScore || previousBest.isEmpty();

    // If new best, clear previous best flag
    if (isNewBest && previousBest.isPresent()) {
      attemptRepository.clearBestAttemptFlag(userId, quizId);
    }

    // Create and save attempt
    QuizAttempt attempt = new QuizAttempt();
    attempt.setQuizId(quizId);
    attempt.setUserId(userId);
    attempt.setScore(score);
    attempt.setTotalPoints(totalPoints);
    attempt.setCorrectCount(correctCount);
    attempt.setTotalCount(totalCount);
    attempt.setDurationSeconds(request.getDurationSeconds());
    attempt.setStartedAt(OffsetDateTime.now().minusSeconds(request.getDurationSeconds()));
    attempt.setCompletedAt(OffsetDateTime.now());
    attempt.setBestAttempt(isNewBest);

    // Add answers to attempt
    for (AttemptAnswer aa : attemptAnswers) {
      attempt.addAnswer(aa);
    }

    QuizAttempt savedAttempt = attemptRepository.save(attempt);

    // Update user's total score if new best
    if (isNewBest) {
      updateUserTotalScore(userId);
    }

    // Build response
    SubmitQuizResponse.Builder responseBuilder = SubmitQuizResponse.newBuilder()
            .setSuccess(true)
            .setScore(score)
            .setTotalPoints(totalPoints)
            .setAttemptId(savedAttempt.getId().toString())
            .setIsNewBest(isNewBest)
            .setPreviousBestScore(previousBestScore);

    for (AnswerResult result : results) {
      responseBuilder.addResults(result);
    }

    return responseBuilder.build();
  }

  /**
   * Legacy method - grade without saving (for backward compatibility)
   */
  @Transactional(readOnly = true)
  public SubmitQuizResponse gradeQuiz(SubmitQuizRequest request) {
    // Now delegates to gradeAndSaveQuiz
    return gradeAndSaveQuiz(request);
  }

  /**
   * Update user's total score (sum of all best attempts)
   */
  @Transactional
  public void updateUserTotalScore(UUID userId) {
    List<QuizAttempt> bestAttempts = attemptRepository.findBestAttemptsByUserId(userId);
    int totalScore = bestAttempts.stream()
            .mapToInt(QuizAttempt::getScore)
            .sum();

    userRepository.findById(userId).ifPresent(user -> {
      user.setTotalScore(totalScore);
      userRepository.save(user);
    });
  }

  /**
   * Get all attempts for a user
   */
  @Transactional(readOnly = true)
  public List<QuizAttempt> getUserAttempts(UUID userId, int limit) {
    List<QuizAttempt> attempts = attemptRepository.findByUserIdOrderByCompletedAtDesc(userId);
    if (limit > 0 && attempts.size() > limit) {
      return attempts.subList(0, limit);
    }
    return attempts;
  }

  /**
   * Get attempts for a specific quiz by a user
   */
  @Transactional(readOnly = true)
  public List<QuizAttempt> getQuizAttempts(UUID userId, UUID quizId) {
    return attemptRepository.findByUserIdAndQuizIdOrderByScoreDesc(userId, quizId);
  }

  /**
   * Get best attempt for a quiz
   */
  @Transactional(readOnly = true)
  public Optional<QuizAttempt> getBestAttempt(UUID userId, UUID quizId) {
    return attemptRepository.findBestAttempt(userId, quizId);
  }

  /**
   * Count unique quizzes attempted by user
   */
  @Transactional(readOnly = true)
  public long countQuizzesTaken(UUID userId) {
    return attemptRepository.findBestAttemptsByUserId(userId).size();
  }

  /**
   * Count total attempts by user
   */
  @Transactional(readOnly = true)
  public long countTotalAttempts(UUID userId) {
    return attemptRepository.findByUserIdOrderByCompletedAtDesc(userId).size();
  }

  /**
   * Count quizzes created by user
   */
  @Transactional(readOnly = true)
  public long countQuizzesCreated(UUID userId) {
    return quizRepository.findByCreatedByWithQuestions(userId).size();
  }
}
