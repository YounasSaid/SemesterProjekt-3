package com.semesterprojekt.quiz;

import jakarta.persistence.*;
import org.hibernate.annotations.CreationTimestamp;
import org.hibernate.annotations.UuidGenerator;

import java.time.OffsetDateTime;
import java.util.ArrayList;
import java.util.List;
import java.util.UUID;

/**
 * Class: Quiz
 * --------------------------------------------
 * Purpose:
 *   Represents a quiz in the database.
 *
 * Stored in table: "quizzes"
 */
@Entity
@Table(name = "quizzes")
public class QuizEntity
{

  @Id
  @UuidGenerator
  @Column( name = "id", nullable = false, updatable = false)
  private UUID id;

  @Column(columnDefinition = "citext", name = "title", nullable = false)
  private String title;

  @Column( name = "created_by", nullable = false)
  private UUID createdBy;

  @CreationTimestamp
  @Column( name = "created_at", nullable = false, updatable = false)
  private OffsetDateTime createdAt;

  @OneToMany(mappedBy = "quiz", cascade = CascadeType.ALL, orphanRemoval = true)
  private List<Question> questions = new ArrayList<>();

  // ---- getters & setters ----
  public UUID getId() { return id; }
  public void setId(UUID id) { this.id = id; }

  public String getTitle() { return title; }
  public void setTitle(String title) { this.title = title; }

  public UUID getCreatedBy() { return createdBy; }
  public void setCreatedBy(UUID createdBy) { this.createdBy = createdBy; }

  public OffsetDateTime getCreatedAt() { return createdAt; }
  public void setCreatedAt(OffsetDateTime createdAt) { this.createdAt = createdAt; }

  public List<Question> getQuestions() { return questions; }
  public void setQuestions(List<Question> questions) { this.questions = questions; }

  public void addQuestion(Question question) {
    questions.add(question);
    question.setQuiz(this);
  }

  public void removeQuestion(Question question) {
    questions.remove(question);
    question.setQuiz(null);
  }
}
