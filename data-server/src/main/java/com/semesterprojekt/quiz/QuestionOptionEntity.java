package com.semesterprojekt.quiz;

import jakarta.persistence.*;
import org.hibernate.annotations.UuidGenerator;

import java.util.UUID;

/**
 * Class: QuestionOption
 * --------------------------------------------
 * Purpose:
 *   Represents an answer option for a question.
 *
 * Stored in table: "question_options"
 */
@Entity
@Table(name = "question_options")
public class QuestionOptionEntity
{

  @Id
  @UuidGenerator
  @Column(columnDefinition = "citext", name = "id", nullable = false, updatable = false)
  private UUID id;

  @Column(columnDefinition = "citext", name = "option_text", nullable = false)
  private String optionText;

  @Column(name = "option_order", nullable = false)
  private int optionOrder;

  @Column(name = "is_correct", nullable = false)
  private boolean isCorrect;

  @ManyToOne(fetch = FetchType.LAZY)
  @JoinColumn(name = "question_id", nullable = false)
  private Question question;

  // ---- getters / setters ----
  public UUID getId() { return id; }
  public void setId(UUID id) { this.id = id; }

  public String getOptionText() { return optionText; }
  public void setOptionText(String optionText) { this.optionText = optionText; }

  public int getOptionOrder() { return optionOrder; }
  public void setOptionOrder(int optionOrder) { this.optionOrder = optionOrder; }

  public boolean isCorrect() { return isCorrect; }
  public void setCorrect(boolean correct) { isCorrect = correct; }

  public Question getQuestion() { return question; }
  public void setQuestion(Question question) { this.question = question; }
}
