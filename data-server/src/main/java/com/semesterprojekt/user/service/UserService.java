package com.semesterprojekt.user.service;

import com.semesterprojekt.user.User;
import com.semesterprojekt.user.UserRepository;
import com.semesterprojekt.user.exception.DuplicateEmailException;
import com.semesterprojekt.user.exception.UserNotFoundException;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.UUID;

/**
 * Class: UserService (UPDATED)
 * --------------------------------------------
 * Tilføjet:
 *   - getById: Hent bruger via UUID
 *   - updatePassword: Opdater password hash
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

  // ========================
  // NY - Get by ID
  // ========================
  /**
   * Henter bruger baseret på UUID.
   * Bruges af profilside til at vise brugerdata.
   */
  @Transactional(readOnly = true)
  public User getById(UUID userId) {
    return repo.findById(userId)
            .orElseThrow(() -> new UserNotFoundException(userId.toString()));
  }

  // ========================
  // NY - Update Password
  // ========================
  /**
   * Opdaterer brugerens password hash.
   * 
   * VIGTIGT: Denne metode forventer at AppServer ALLEREDE har verificeret
   * brugerens nuværende password via BCrypt.Verify().
   * 
   * @param userId ID på brugeren
   * @param newPasswordHash Ny password hash (allerede hashet + verificeret af AppServer)
   * @throws UserNotFoundException hvis brugeren ikke findes
   */
  @Transactional
  public void updatePassword(UUID userId, String newPasswordHash) {
    User user = repo.findById(userId)
            .orElseThrow(() -> new UserNotFoundException(userId.toString()));

    user.setPasswordHash(newPasswordHash);
    repo.save(user);
  }
}
