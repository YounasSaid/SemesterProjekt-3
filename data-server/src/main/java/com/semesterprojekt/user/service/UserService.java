package com.semesterprojekt.user.service;

import com.semesterprojekt.user.User;
import com.semesterprojekt.user.UserRepository;
import com.semesterprojekt.user.exception.DuplicateEmailException;
import com.semesterprojekt.user.exception.UserNotFoundException;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;
/**
 * Class: UserService
 * --------------------------------------------
 * Formål:
 *   Indeholder forretningslogikken for User-objekter.
 *   Her tjekkes dubletter, og her gemmes og hentes brugere via UserRepository.
 *
 * Vigtigste metoder:
 *   - createUser(email, firstName, lastName, passwordHash, semester)
 *       → Tjekker først om email allerede findes (case-insensitive).
 *         Hvis den findes → kaster DuplicateEmailException.
 *         Ellers gemmes en ny bruger i databasen.
 *
 *   - getByEmail(email)
 *       → Finder en bruger baseret på email.
 *         Hvis ingen findes → kaster UserNotFoundException.
 *
 * Bruges af:
 *   - gRPC-laget (UserGrpcService), som kalder disse metoder
 *     når klienten opretter eller henter brugere.
 *
 * Bemærk:
 *   @Transactional sikrer, at ændringer i databasen sker som én samlet handling:
 *   enten lykkes alt, eller alt fortrydes.
 */

@Service
public class UserService {

  private final UserRepository repo;

  public UserService(UserRepository repo) {
    this.repo = repo;
  }

  @Transactional
  public User createUser(String email, String firstName, String lastName, String passwordHash, short semester) {
    if (repo.existsByEmailIgnoreCase(email)) {
      throw new DuplicateEmailException(email);
    }
    User u = new User();
    u.setEmail(email);
    u.setFirstName(firstName);
    u.setLastName(lastName);
    u.setPasswordHash(passwordHash);
    u.setSemester(semester);
    return repo.save(u);
  }

  @Transactional(readOnly = true)
  public User getByEmail(String email) {
    return repo.findByEmailIgnoreCase(email)
            .orElseThrow(() -> new UserNotFoundException(email));
  }
}
