package com.semesterprojekt.quiz;

import org.springframework.data.jpa.repository.JpaRepository;

import java.util.Optional;
import java.util.UUID;

public interface QuizRepository extends JpaRepository<Quiz, UUID>
{
  Optional<Quiz> findByTitleIgnoreCase(String title);
  boolean existsByTitleNameIgnoreCase(String title);
}
