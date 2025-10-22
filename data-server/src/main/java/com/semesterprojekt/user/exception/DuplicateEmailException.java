package com.semesterprojekt.user.exception;
/**
 * Class: DuplicateEmailException
 * --------------------------------------------
 * Formål:
 *   Bruges til at kaste en klar fejlbesked, hvis en bruger forsøger
 *   at oprette en konto med en email, der allerede findes i databasen.
 *
 * Bruges af:
 *   - UserService.createUser()
 *
 * Effekt:
 *   Når denne exception kastes, kan gRPC-laget fange den og returnere
 *   fejlkoden "ALREADY_EXISTS" til klienten.
 */


public class DuplicateEmailException extends RuntimeException {
  public DuplicateEmailException(String email) {
    super("Email already exists: " + email);
  }
}
