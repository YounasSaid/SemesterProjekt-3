/**
 * Class: UserGrpcService
 * --------------------------------------------
 * Formål:
 *   Implementerer gRPC-laget for brugerhåndtering.
 *   Denne klasse modtager kald fra App-serveren via gRPC,
 *   kalder UserService for at udføre logikken,
 *   og returnerer svar eller fejl tilbage gennem gRPC-protokollen.
 *
 * Funktion i systemet:
 *   App-serveren (C#) kalder metoder som CreateUser og GetUserByEmail
 *   gennem denne gRPC-service, som igen arbejder med databasen via UserService.
 *
 * Vigtigste metoder:
 *   - createUser(request, responseObserver)
 *       → Modtager en ny brugers data fra App-serveren.
 *         Tjekker dublet via UserService.
 *         Hvis email findes → returnerer fejlkode "ALREADY_EXISTS".
 *         Hvis ikke → gemmer brugeren og returnerer user_id.
 *
 *   - getUserByEmail(request, responseObserver)
 *       → Finder bruger via UserService.getByEmail().
 *         Hvis bruger ikke findes → returnerer fejlkode "NOT_FOUND".
 *         Ellers sendes brugerdata (id, email, passwordHash, semester) tilbage.
 *
 * Bruges af:
 *   - App-serveren (via gRPC client stub)
 *
 * Bemærk:
 *   - Bruger klasser auto-genereret fra proto-filen (fx UserServiceGrpc, CreateUserRequest, osv.).
 *   - gRPC-statusser som ALREADY_EXISTS og NOT_FOUND svarer til exceptions i UserService.
 *   - Denne klasse har ingen databaseadgang selv — alt går gennem UserService.
 */

package com.semesterprojekt.grpc;

import com.semesterprojekt.user.User;
import com.semesterprojekt.user.exception.DuplicateEmailException;
import com.semesterprojekt.user.exception.UserNotFoundException;
import com.semesterprojekt.user.service.UserService;

// ↓↓↓ Ret disse imports hvis jeres java_package i .proto er anderledes ↓↓↓
import com.semesterprojekt.proto.user.CreateUserRequest;
import com.semesterprojekt.proto.user.CreateUserResponse;
import com.semesterprojekt.proto.user.GetUserByEmailRequest;
import com.semesterprojekt.proto.user.GetUserByEmailResponse;
import com.semesterprojekt.proto.user.UserServiceGrpc;
// ↑↑↑ Ret disse imports hvis jeres java_package i .proto er anderledes ↑↑↑

import io.grpc.Status;
import io.grpc.stub.StreamObserver;
import net.devh.boot.grpc.server.service.GrpcService;

import java.util.UUID;

@GrpcService
public class UserGrpcService extends UserServiceGrpc.UserServiceImplBase {

  private final UserService userService;

  public UserGrpcService(UserService userService) {
    this.userService = userService;
  }

  @Override
  public void createUser(CreateUserRequest request, StreamObserver<CreateUserResponse> responseObserver) {
    try {
      // 1) Udtræk felter fra request
      final String email = request.getEmail();
      final String firstName = request.getFirstName();
      final String lastName = request.getLastName();
      final String passwordHash = request.getPasswordHash();
      final short semester = (short) request.getSemester(); // proto int32 → Java short

      // 2) Kald domænelaget
      User saved = userService.createUser(email, firstName, lastName, passwordHash, semester);

      // 3) Byg gRPC-svar
      CreateUserResponse response = CreateUserResponse.newBuilder()
              .setUserId(saved.getId() != null ? saved.getId().toString() : "")
              .build();

      // 4) Send svar
      responseObserver.onNext(response);
      responseObserver.onCompleted();

    } catch (DuplicateEmailException dup) {
      // E-mail findes allerede → ALREADY_EXISTS
      responseObserver.onError(
              Status.ALREADY_EXISTS
                      .withDescription(dup.getMessage())
                      .asRuntimeException()
      );
    } catch (Exception ex) {
      // Uventet fejl → INTERNAL
      responseObserver.onError(
              Status.INTERNAL
                      .withDescription("Unexpected error in createUser: " + ex.getMessage())
                      .asRuntimeException()
      );
    }
  }

  @Override
  public void getUserByEmail(GetUserByEmailRequest request, StreamObserver<GetUserByEmailResponse> responseObserver) {
    try {
      final String email = request.getEmail();

      User u = userService.getByEmail(email);

      // Map til gRPC-respons
      GetUserByEmailResponse response = GetUserByEmailResponse.newBuilder()
              .setFound(true)
              .setUserId(u.getId() != null ? u.getId().toString() : "")
              .setEmail(u.getEmail() != null ? u.getEmail() : "")
              .setPasswordHash(u.getPasswordHash() != null ? u.getPasswordHash() : "")
              .setSemester(u.getSemester())
              .build();

      responseObserver.onNext(response);
      responseObserver.onCompleted();

    } catch (UserNotFoundException nf) {
      responseObserver.onError(
              Status.NOT_FOUND
                      .withDescription(nf.getMessage())
                      .asRuntimeException()
      );
    } catch (Exception ex) {
      responseObserver.onError(
              Status.INTERNAL
                      .withDescription("Unexpected error in getUserByEmail: " + ex.getMessage())
                      .asRuntimeException()
      );
    }
  }
}
