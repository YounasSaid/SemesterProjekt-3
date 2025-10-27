package com.semesterprojekt.user.exception;
/**
 * Class: UserNotFoundException
 * --------------------------------------------
 * Formål:
 *   Kastes, hvis systemet forsøger at finde en bruger i databasen via email,
 *   men ingen bruger findes med den adresse.
 *
 * Bruges af:
 *   - UserService.getByEmail()
 *
 * Effekt:
 *   gRPC-laget kan fange denne fejl og returnere "NOT_FOUND" til klienten.
 */


public class UserNotFoundException extends RuntimeException {
  public UserNotFoundException(String email) {
    super("User not found for email: " + email);
  }
}
