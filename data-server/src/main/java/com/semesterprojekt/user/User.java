package com.semesterprojekt.user;

import jakarta.persistence.*;
import org.hibernate.annotations.CreationTimestamp;
import org.hibernate.annotations.UuidGenerator;

import java.time.OffsetDateTime;
import java.util.UUID;
/**
 * Class: User
 * --------------------------------------------
 * Formål:
 *   Repræsenterer en bruger i databasen (ORM-entity via Hibernate).
 *
 * Gemmes i tabellen: "users"
 *
 * Vigtigste felter:
 *   - id: Primærnøgle (UUID genereres automatisk)
 *   - email, firstName, lastName, passwordHash, semester
 *   - createdAt: tidspunkt for oprettelse
 *
 * Bruges af:
 *   - UserRepository (for at hente/gemme brugere)
 *   - UserService (forretningslogik omkring oprettelse og opslag)
 *
 * Bemærk:
 *   Hibernate sørger for at mappe Java-felterne til kolonner i databasen.
 */
@Entity
@Table(name = "users", indexes = {
        @Index(name = "ux_users_email_lower", columnList = "email", unique = false) // DB har unik index på lower(email)
})
public class User {

  @Id
  @UuidGenerator
  @Column(name = "id", nullable = false, updatable = false)
  private UUID id;

  @Column(name = "email", nullable = false)
  private String email;

  @Column(name = "first_name", nullable = false)
  private String firstName;

  @Column(name = "last_name", nullable = false)
  private String lastName;

  @Column(name = "password_hash", nullable = false)
  private String passwordHash;

  @Column(name = "semester", nullable = false)
  private short semester;

  @CreationTimestamp
  @Column(name = "created_at", nullable = false, updatable = false)
  private OffsetDateTime createdAt;

  // --- getters/setters ---

  public UUID getId() { return id; }
  public void setId(UUID id) { this.id = id; }

  public String getEmail() { return email; }
  public void setEmail(String email) { this.email = email; }

  public String getFirstName() { return firstName; }
  public void setFirstName(String firstName) { this.firstName = firstName; }

  public String getLastName() { return lastName; }
  public void setLastName(String lastName) { this.lastName = lastName; }

  public String getPasswordHash() { return passwordHash; }
  public void setPasswordHash(String passwordHash) { this.passwordHash = passwordHash; }

  public short getSemester() { return semester; }
  public void setSemester(short semester) { this.semester = semester; }

  public OffsetDateTime getCreatedAt() { return createdAt; }
  public void setCreatedAt(OffsetDateTime createdAt) { this.createdAt = createdAt; }
}
