package com.semesterprojekt.quiz;

import org.springframework.data.jpa.repository.EntityGraph;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

public interface QuizRepository extends JpaRepository<QuizEntity, UUID> {
  // EAGERLY fetch questions to avoid LazyInitializationException
  @Query("SELECT DISTINCT q FROM QuizEntity q LEFT JOIN FETCH q.questions WHERE q.createdBy = :userId")
  List<QuizEntity> findByCreatedByWithQuestions(@Param("userId") UUID userId);

  // Use @EntityGraph to fetch both questions and options (avoids MultipleBagFetchException)
  @EntityGraph(attributePaths = {"questions", "questions.options"})
  Optional<QuizEntity> findById(UUID id);

  // Add to QuizRepository.java
  @Query("SELECT DISTINCT q FROM QuizEntity q LEFT JOIN FETCH q.questions WHERE q.createdBy != :userId")
  List<QuizEntity> findByCreatedByNotWithQuestions(@Param("userId") UUID userId);
}