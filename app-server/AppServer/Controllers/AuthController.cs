using AppServer.Models;
using AppServer.Utils;
using AppServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IPasswordHasher _hasher;
    private readonly UserGrpcClient _grpcClient;

    public AuthController(IPasswordHasher hasher, UserGrpcClient grpcClient)
    {
        _hasher = hasher;
        _grpcClient = grpcClient;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        // Kun semester 3 er tilladt i denne POC
        if (req.Semester != 3)
            return BadRequest(new { code = "INVALID_SEMESTER" });

        var email = req.SchoolEmail.Trim().ToLowerInvariant();
        var passwordHash = _hasher.Hash(req.Password);

        // Kald Data Server via gRPC
        var (success, userId, errorCode) = await _grpcClient.CreateUserAsync(
            email, 
            req.FirstName, 
            req.LastName, 
            passwordHash, 
            req.Semester
        );

        if (!success)
        {
            if (errorCode == "EMAIL_TAKEN")
                return Conflict(new { code = "EMAIL_TAKEN" });
            
            return StatusCode(500, new { code = "INTERNAL_ERROR" });
        }

        return StatusCode(201, new RegisterResponse { UserId = Guid.Parse(userId!) });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var email = req.SchoolEmail.Trim().ToLowerInvariant();

        // Hent bruger fra Data Server via gRPC
        var (found, userId, userEmail, passwordHash, semester) = 
            await _grpcClient.GetUserByEmailAsync(email);

        if (!found)
            return Unauthorized(new { code = "INVALID_CREDENTIALS" });

        // Verificer password
        if (!_hasher.Verify(req.Password, passwordHash!))
            return Unauthorized(new { code = "INVALID_CREDENTIALS" });

        // Gem bruger-id i session
        HttpContext.Session.SetString(SessionKeys.UserId, userId!);

        return Ok(new LoginResponse { UserId = Guid.Parse(userId!) });
    }
}