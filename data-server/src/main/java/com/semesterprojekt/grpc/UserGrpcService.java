/**
 * Class: UserGrpcService (UPDATED)
 * --------------------------------------------
 * Tilføjet:
 *   - getUserById: Hent bruger via user_id (til profilside)
 *   - updatePassword: Opdater brugerens adgangskode
 */

package com.semesterprojekt.grpc;

import com.semesterprojekt.user.User;
import com.semesterprojekt.user.exception.DuplicateEmailException;
import com.semesterprojekt.user.exception.UserNotFoundException;
import com.semesterprojekt.user.service.UserService;

import com.semesterprojekt.proto.user.CreateUserRequest;
import com.semesterprojekt.proto.user.CreateUserResponse;
import com.semesterprojekt.proto.user.GetUserByEmailRequest;
import com.semesterprojekt.proto.user.GetUserByEmailResponse;
import com.semesterprojekt.proto.user.GetUserByIdRequest;
import com.semesterprojekt.proto.user.GetUserByIdResponse;
import com.semesterprojekt.proto.user.UpdatePasswordRequest;
import com.semesterprojekt.proto.user.UpdatePasswordResponse;
import com.semesterprojekt.proto.user.UserServiceGrpc;

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
      final String email = request.getEmail();
      final String firstName = request.getFirstName();
      final String lastName = request.getLastName();
      final String passwordHash = request.getPasswordHash();
      final short semester = (short) request.getSemester();

      User saved = userService.createUser(email, firstName, lastName, passwordHash, semester);

      CreateUserResponse response = CreateUserResponse.newBuilder()
              .setUserId(saved.getId() != null ? saved.getId().toString() : "")
              .build();

      responseObserver.onNext(response);
      responseObserver.onCompleted();

    } catch (DuplicateEmailException dup) {
      responseObserver.onError(
              Status.ALREADY_EXISTS
                      .withDescription(dup.getMessage())
                      .asRuntimeException()
      );
    } catch (Exception ex) {
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

  // ========================
  // NY - Get User By ID
  // ========================
  @Override
  public void getUserById(GetUserByIdRequest request, StreamObserver<GetUserByIdResponse> responseObserver) {
    try {
      final UUID userId = UUID.fromString(request.getUserId());

      User u = userService.getById(userId);

      GetUserByIdResponse response = GetUserByIdResponse.newBuilder()
              .setFound(true)
              .setUserId(u.getId() != null ? u.getId().toString() : "")
              .setEmail(u.getEmail() != null ? u.getEmail() : "")
              .setFirstName(u.getFirstName() != null ? u.getFirstName() : "")
              .setLastName(u.getLastName() != null ? u.getLastName() : "")
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
                      .withDescription("Unexpected error in getUserById: " + ex.getMessage())
                      .asRuntimeException()
      );
    }
  }

  // ========================
  // NY - Update Password
  // ========================
  @Override
  public void updatePassword(UpdatePasswordRequest request, StreamObserver<UpdatePasswordResponse> responseObserver) {
    try {
      final UUID userId = UUID.fromString(request.getUserId());
      final String newPasswordHash = request.getNewPasswordHash();

      userService.updatePassword(userId, newPasswordHash);

      UpdatePasswordResponse response = UpdatePasswordResponse.newBuilder()
              .setSuccess(true)
              .setErrorCode("")
              .build();

      responseObserver.onNext(response);
      responseObserver.onCompleted();

    } catch (UserNotFoundException nf) {
      UpdatePasswordResponse response = UpdatePasswordResponse.newBuilder()
              .setSuccess(false)
              .setErrorCode("USER_NOT_FOUND")
              .build();
      responseObserver.onNext(response);
      responseObserver.onCompleted();
    } catch (Exception ex) {
      responseObserver.onError(
              Status.INTERNAL
                      .withDescription("Unexpected error in updatePassword: " + ex.getMessage())
                      .asRuntimeException()
      );
    }
  }
}
