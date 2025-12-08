using AppServer.Models;
using AppServer.Services;
using AppServer.Utils;
using Microsoft.AspNetCore.Mvc;

namespace AppServer.Controllers;

// REST API controller til brugerprofil operationer (hent profil, skift password)
[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly UserGrpcClient _userClient;
    private readonly IPasswordHasher _hasher;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(
        UserGrpcClient userClient, 
        IPasswordHasher hasher,
        ILogger<ProfileController> logger)
    {
        _userClient = userClient;
        _hasher = hasher;
        _logger = logger;
    }

    // Henter brugerens profildata
    [HttpGet]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile()
    {
        // Tjek autentificering fra COOKIE
        var userIdString = HttpContext.Request.Cookies[CookieKeys.UserId];
        if (string.IsNullOrEmpty(userIdString))
        {
            _logger.LogWarning("Unauthenticated user attempted to get profile");
            return Unauthorized(new { error = "Not authenticated" });
        }

        if (!Guid.TryParse(userIdString, out var userId))
        {
            _logger.LogError("Invalid userId in cookie: {UserId}", userIdString);
            return Unauthorized(new { error = "Not authenticated" });
        }

        try
        {
            // Kald gRPC klient
            var profile = await _userClient.GetUserByIdAsync(userId);

            if (profile == null)
            {
                _logger.LogWarning("User not found: {UserId}", userId);
                return NotFound(new { error = "User not found" });
            }

            _logger.LogInformation("Profile retrieved for user {UserId}", userId);
            return Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving profile for user {UserId}", userId);
            return StatusCode(500, new { error = "An error occurred while retrieving profile" });
        }
    }

    // Opdaterer brugerens password
    [HttpPut("password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for password update");
            return ValidationProblem(ModelState);
        }

        // Tjek autentificering fra COOKIE
        var userIdString = HttpContext.Request.Cookies[CookieKeys.UserId];
        if (string.IsNullOrEmpty(userIdString))
        {
            _logger.LogWarning("Unauthenticated user attempted to update password");
            return Unauthorized(new { error = "Not authenticated" });
        }

        if (!Guid.TryParse(userIdString, out var userId))
        {
            _logger.LogError("Invalid userId in cookie: {UserId}", userIdString);
            return Unauthorized(new { error = "Not authenticated" });
        }

        try
        {
            // TRIN 1: Hent bruger fra DataServer for at få passwordHash
            var (found, _, email, storedPasswordHash, _) = await _userClient.GetUserByEmailAsync(request.Email);

            if (!found || storedPasswordHash == null)
            {
                _logger.LogWarning("User not found during password update: {Email}", request.Email);
                return BadRequest(new { error = "WRONG_PASSWORD" });
            }

            // TRIN 2: Verificer currentPassword (BCrypt verification i AppServer)
            if (!_hasher.Verify(request.CurrentPassword, storedPasswordHash))
            {
                _logger.LogWarning("Wrong current password for user {Email}", request.Email);
                return BadRequest(new { error = "WRONG_PASSWORD" });
            }

            // TRIN 3: Hash det nye password
            var newPasswordHash = _hasher.Hash(request.NewPassword);

            // TRIN 4: Opdater password i DataServer
            var (success, errorCode) = await _userClient.UpdatePasswordAsync(userId, newPasswordHash);

            if (success)
            {
                _logger.LogInformation("Password updated successfully for user {UserId}", userId);
                return Ok(new { success = true, message = "Password updated successfully" });
            }
            else
            {
                _logger.LogWarning("Failed to update password for user {UserId}: {ErrorCode}", userId, errorCode);
                return BadRequest(new { error = errorCode ?? "UNKNOWN_ERROR" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating password for user {UserId}", userId);
            return StatusCode(500, new { error = "An error occurred while updating password" });
        }
    }
}
