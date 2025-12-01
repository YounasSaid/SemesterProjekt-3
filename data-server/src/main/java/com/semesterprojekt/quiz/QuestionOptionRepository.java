package com.semesterprojekt.quiz;

import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;
import java.util.UUID;

public interface QuestionOptionRepository extends JpaRepository<QuestionOptionEntity, UUID> {

  List<QuestionOptionEntity> findByQuestionId(UUID questionId);
}
