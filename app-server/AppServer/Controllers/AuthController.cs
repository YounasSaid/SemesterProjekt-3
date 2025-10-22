using System.Collections.Concurrent;
using AppServer.Models;           // <-- vigtigt for RegisterRequest/LoginRequest
using AppServer.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace AppServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IPasswordHasher _hasher;

    private static readonly ConcurrentDictionary<string, UserRow> Users = new();

    public AuthController(IPasswordHasher hasher)
    {
        _hasher = hasher;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest req)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        // Kun semester 3 er tilladt i denne POC
        if (req.Semester != 3)
            return BadRequest(new { code = "INVALID_SEMESTER" });

        var email = req.SchoolEmail.Trim().ToLowerInvariant();

        if (Users.ContainsKey(email))
            return Conflict(new { code = "EMAIL_TAKEN" });

        var user = new UserRow
        {
            UserId = Guid.NewGuid(),
            FirstName = req.FirstName,
            LastName = req.LastName,
            Email = email,
            PasswordHash = _hasher.Hash(req.Password),
            Semester = req.Semester
        };

        Users[email] = user;

        return StatusCode(201, new RegisterResponse { UserId = user.UserId });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest req)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var email = req.SchoolEmail.Trim().ToLowerInvariant();

        if (!Users.TryGetValue(email, out var user))
            return Unauthorized(new { code = "INVALID_CREDENTIALS" });

        if (!_hasher.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { code = "INVALID_CREDENTIALS" });

        // Gem bruger-id i session
        HttpContext.Session.SetString(SessionKeys.UserId, user.UserId.ToString());

        return Ok(new LoginResponse { UserId = user.UserId });
    }

    private class UserRow
    {
        public Guid UserId { get; init; }
        public string Email { get; init; } = "";
        public string FirstName { get; init; } = "";
        public string LastName { get; init; } = "";
        public string PasswordHash { get; init; } = "";
        public int Semester { get; init; }
    }
}
