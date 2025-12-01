package com.semesterprojekt.quiz.service;

import com.semesterprojekt.quiz.Question;
import com.semesterprojekt.quiz.Quiz;
import com.semesterprojekt.quiz.QuizRepository;
import com.semesterprojekt.quiz.exception.QuizNotFoundException;
import org.springframework.transaction.annotation.Transactional;
import org.springframework.stereotype.Service;

import java.util.List;

@Service
public class CreateQuizService
{
  private final QuizRepository repo;

  public CreateQuizService(QuizRepository repo) {this.repo = repo;}

  public Quiz createQuiz(String title, List<Question> questions)
  {
    Quiz q = new Quiz();
    q.setTitle(title);
    q.setQuestions(questions);
    return repo.save(q);
  }

  @Transactional(readOnly = true)
  public Quiz getByTitle(String title) {
    return repo.findByTitleIgnoreCase(title).orElseThrow(() -> new QuizNotFoundException(title));
  }
}
