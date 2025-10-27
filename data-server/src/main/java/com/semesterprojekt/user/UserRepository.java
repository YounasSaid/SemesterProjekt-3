package com.semesterprojekt.user;

import org.springframework.data.jpa.repository.JpaRepository;
import java.util.Optional;
import java.util.UUID;
/**
 * Class: UserRepository
 * --------------------------------------------
 * Formål:
 *   Repositoriet håndterer al databasekommunikation for User-entiteten.
 *   Det udnytter Spring Data JPA, så vi slipper for at skrive SQL selv.
 *
 * Vigtigste metoder:
 *   - findByEmailIgnoreCase(String email)
 *       → Finder en bruger baseret på email (uden forskel på store/små bogstaver).
 *
 *   - existsByEmailIgnoreCase(String email)
 *       → Returnerer true, hvis der allerede findes en bruger med den email.
 *
 * Bruges af:
 *   - UserService (som bruger metoderne til at oprette og finde brugere)
 *
 * Bemærk:
 *   JPA laver automatisk SQL-forespørgsler ud fra metodernes navne.
 */


public interface UserRepository extends JpaRepository<User, UUID> {
  Optional<User> findByEmailIgnoreCase(String email);
  boolean existsByEmailIgnoreCase(String email);
}
