package com.semesterprojekt.quiz;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Modifying;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

public interface QuizAttemptRepository extends JpaRepository<QuizAttempt, UUID> {

    /**
     * Find alle forsøg for en bruger på en specifik quiz (sorteret efter score, højeste først)
     */
    @Query("SELECT a FROM QuizAttempt a WHERE a.userId = :userId AND a.quizId = :quizId ORDER BY a.score DESC")
    List<QuizAttempt> findByUserIdAndQuizIdOrderByScoreDesc(
            @Param("userId") UUID userId, 
            @Param("quizId") UUID quizId);

    /**
     * Find bedste forsøg for en bruger på en specifik quiz
     */
    @Query("SELECT a FROM QuizAttempt a WHERE a.userId = :userId AND a.quizId = :quizId AND a.isBestAttempt = true")
    Optional<QuizAttempt> findBestAttempt(
            @Param("userId") UUID userId, 
            @Param("quizId") UUID quizId);

    /**
     * Find alle forsøg for en bruger (sorteret efter dato, nyeste først)
     */
    @Query("SELECT a FROM QuizAttempt a WHERE a.userId = :userId ORDER BY a.completedAt DESC")
    List<QuizAttempt> findByUserIdOrderByCompletedAtDesc(@Param("userId") UUID userId);

    /**
     * Find alle bedste forsøg for en bruger (til totalscore beregning)
     */
    @Query("SELECT a FROM QuizAttempt a WHERE a.userId = :userId AND a.isBestAttempt = true")
    List<QuizAttempt> findBestAttemptsByUserId(@Param("userId") UUID userId);

    /**
     * Fjern "best attempt" flag fra alle forsøg for en bruger på en quiz
     */
    @Modifying
    @Query("UPDATE QuizAttempt a SET a.isBestAttempt = false WHERE a.userId = :userId AND a.quizId = :quizId")
    void clearBestAttemptFlag(@Param("userId") UUID userId, @Param("quizId") UUID quizId);

    /**
     * Tæl antal forsøg for en bruger på en quiz
     */
    @Query("SELECT COUNT(a) FROM QuizAttempt a WHERE a.userId = :userId AND a.quizId = :quizId")
    long countByUserIdAndQuizId(@Param("userId") UUID userId, @Param("quizId") UUID quizId);

    /**
     * Find seneste forsøg for en bruger
     */
    @Query("SELECT a FROM QuizAttempt a WHERE a.userId = :userId ORDER BY a.completedAt DESC LIMIT 5")
    List<QuizAttempt> findRecentAttempts(@Param("userId") UUID userId);
}
