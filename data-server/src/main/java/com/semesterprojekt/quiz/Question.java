package com.semesterprojekt.quiz;

import jakarta.persistence.*;
import org.hibernate.annotations.UuidGenerator;

import java.util.UUID;

/**
 * Class: Question
 * --------------------------------------------
 * Purpose:
 *   Represents a question belonging to a quiz.
 *
 * Stored in table: "questions"
 *
 * Fields:
 *   - id: Primary key (UUID generated)
 *   - text: Question text
 *   - options: Stored as a comma-separated string or JSON (depending on your choice)
 *   - correctOptionIndex: Which option is correct
 *   - quiz: Many-to-one relation to Quiz
 */
@Entity
@Table(name = "questions")
public class Question {

  @Id
  @UuidGenerator
  @Column(columnDefinition = "citext", name = "id", nullable = false, updatable = false)
  private UUID id;

  @Column(columnDefinition = "citext", name = "text", nullable = false)
  private String text;

  @Column(columnDefinition = "citext", name = "options", nullable = false)
  private String options; // You can change this to JSON if preferred

  @Column(columnDefinition = "citext", name = "correct_option_index", nullable = false)
  private short correctOptionIndex;

  @ManyToOne
  @JoinColumn(name = "quiz_id", nullable = false)
  private Quiz quiz;

  // --- getters/setters ---

  public UUID getId() { return id; }
  public void setId(UUID id) { this.id = id; }

  public String getText() { return text; }
  public void setText(String text) { this.text = text; }

  public String getOptions() { return options; }
  public void setOptions(String options) { this.options = options; }

  public short getCorrectOptionIndex() { return correctOptionIndex; }
  public void setCorrectOptionIndex(short correctOptionIndex) {
    this.correctOptionIndex = correctOptionIndex;
  }

  public Quiz getQuiz() { return quiz; }
  public void setQuiz(Quiz quiz) { this.quiz = quiz; }
}
