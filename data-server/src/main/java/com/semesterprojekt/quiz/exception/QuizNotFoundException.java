package com.semesterprojekt.quiz.exception;

public class QuizNotFoundException extends RuntimeException {

  public QuizNotFoundException(String title) {
    super("Quiz not found with title: " + title);
  }

  public QuizNotFoundException(java.util.UUID id) {
    super("Quiz not found with id: " + id);
  }

  public QuizNotFoundException(String field, String value) {
    super("Quiz not found (" + field + ": " + value + ")");
  }
}
