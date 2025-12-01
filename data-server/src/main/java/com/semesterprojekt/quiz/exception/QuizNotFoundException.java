package com.semesterprojekt.quiz.exception;

public class QuizNotFoundException extends RuntimeException
{
  public QuizNotFoundException(String title) {
    super("Quiz not found for title: " + title);
  }
}
