package com.semesterprojekt.quiz.service;

import com.semesterprojekt.quiz.Question;
import com.semesterprojekt.quiz.QuestionOptionEntity;
import com.semesterprojekt.quiz.QuizEntity;
import com.semesterprojekt.quiz.QuizRepository;
import com.semesterprojekt.proto.quiz.*;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.ArrayList;
import java.util.List;
import java.util.UUID;

@Service
public class QuizService {

  private final QuizRepository quizRepository;

  public QuizService(QuizRepository quizRepository) {
    this.quizRepository = quizRepository;
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

  public int calculateTotalPoints(QuizEntity quiz) {
    return quiz.getQuestions().stream()
            .mapToInt(Question::getPoints)
            .sum();
  }

  @Transactional(readOnly = true)
  public SubmitQuizResponse gradeQuiz(SubmitQuizRequest request) {
    UUID quizId = UUID.fromString(request.getQuizId());
    QuizEntity quiz = quizRepository.findById(quizId).orElse(null);

    if (quiz == null) {
      return SubmitQuizResponse.newBuilder()
              .setSuccess(false)
              .setScore(0)
              .setTotalPoints(0)
              .build();
    }

    int score = 0;
    int totalPoints = calculateTotalPoints(quiz);
    SubmitQuizResponse.Builder responseBuilder = SubmitQuizResponse.newBuilder();

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

        responseBuilder.addResults(
                AnswerResult.newBuilder()
                        .setQuestionId(questionId.toString())
                        .setIsCorrect(isCorrect)
                        .setPointsEarned(pointsEarned)
        );
      }
    }

    return responseBuilder
            .setSuccess(true)
            .setScore(score)
            .setTotalPoints(totalPoints)
            .build();
  }
}