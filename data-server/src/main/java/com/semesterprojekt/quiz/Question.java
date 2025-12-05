package com.semesterprojekt.quiz;

import jakarta.persistence.*;
import org.hibernate.annotations.UuidGenerator;

import java.util.ArrayList;
import java.util.List;
import java.util.UUID;

/**
 * Class: Question
 * --------------------------------------------
 * Purpose:
 *   Represents a question in a quiz.
 *
 * Stored in table: "questions"
 */
@Entity
@Table(name = "quiz_questions")
public class Question
{

  @Id
  @UuidGenerator
  @Column( name = "id", nullable = false, updatable = false)
  private UUID id;

  @Column( name = "question_text", nullable = false)
  private String questionText;

  @Column(name = "question_order", nullable = false)
  private int questionOrder;

  @Column(name = "points", nullable = false)
  private int points;

  @ManyToOne(fetch = FetchType.LAZY)
  @JoinColumn(name = "quiz_id", nullable = false)
  private QuizEntity quiz;

  @OneToMany(mappedBy = "question", cascade = CascadeType.ALL, orphanRemoval = true)
  private List<QuestionOptionEntity> options = new ArrayList<>();

  // ---- getters / setters ----
  public UUID getId() { return id; }
  public void setId(UUID id) { this.id = id; }

  public String getQuestionText() { return questionText; }
  public void setQuestionText(String questionText) { this.questionText = questionText; }

  public int getQuestionOrder() { return questionOrder; }
  public void setQuestionOrder(int questionOrder) { this.questionOrder = questionOrder; }

  public int getPoints() { return points; }
  public void setPoints(int points) { this.points = points; }

  public QuizEntity getQuiz() { return quiz; }
  public void setQuiz(QuizEntity quiz) { this.quiz = quiz; }

  public List<QuestionOptionEntity> getOptions() { return options; }
  public void setOptions(List<QuestionOptionEntity> options) { this.options = options; }

  public void addOption(QuestionOptionEntity option) {
    options.add(option);
    option.setQuestion(this);
  }

  public void removeOption(QuestionOptionEntity option) {
    options.remove(option);
    option.setQuestion(null);
  }
}
