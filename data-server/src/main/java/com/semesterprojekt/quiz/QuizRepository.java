package com.semesterprojekt.quiz;

import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

public interface QuizRepository extends JpaRepository<QuizEntity, UUID> {

  Optional<QuizEntity> findByTitleIgnoreCase(String title);

  // Required by: GetUserQuizzesRequest (proto)
  List<QuizEntity> findByCreatedBy(String createdBy);
}
