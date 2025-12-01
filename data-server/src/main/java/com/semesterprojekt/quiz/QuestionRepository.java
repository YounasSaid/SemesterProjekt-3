package com.semesterprojekt.quiz;

import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;
import java.util.UUID;

public interface QuestionRepository extends JpaRepository<Question, UUID> {

  List<Question> findByQuizId(UUID quizId);
}
