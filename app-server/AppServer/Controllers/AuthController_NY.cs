using AppServer.Models;
using AppServer.Utils;
using Microsoft.AspNetCore.Mvc;
using Grpc.Net.Client;
using SemesterProjekt.Proto.User;

namespace AppServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IPasswordHasher _hasher;
    private readonly GrpcChannel _grpcChannel;
    private readonly UserService.UserServiceClient _userClient;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IPasswordHasher hasher, ILogger<AuthController> logger)
    {
        _hasher = hasher;
        _logger = logger;
        
        _grpcChannel = GrpcChannel.ForAddress("http://localhost:9090");
        _userClient = new UserService.UserServiceClient(_grpcChannel);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (req.Semester != 3)
            return BadRequest(new { code = "INVALID_SEMESTER" });

        try
        {
            var passwordHash = _hasher.Hash(req.Password);

            var grpcRequest = new CreateUserRequest
            {
                Email = req.SchoolEmail.Trim().ToLowerInvariant(),
                FirstName = req.FirstName,
                LastName = req.LastName,
                PasswordHash = passwordHash,
                Semester = req.Semester
            };

            var grpcResponse = await _userClient.CreateUserAsync(grpcRequest);

            return StatusCode(201, new RegisterResponse 
            { 
                UserId = Guid.Parse(grpcResponse.UserId) 
            });
        }
        catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.AlreadyExists)
        {
            return Conflict(new { code = "EMAIL_TAKEN" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Register Error");
            return StatusCode(500, new { code = "SERVER_ERROR" });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        _logger.LogInformation("=== LOGIN START ===");
        
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        try
        {
            var email = req.SchoolEmail.Trim().ToLowerInvariant();

            var grpcRequest = new GetUserByEmailRequest { Email = email };
            var grpcResponse = await _userClient.GetUserByEmailAsync(grpcRequest);

            if (!grpcResponse.Found)
                return Unauthorized(new { code = "INVALID_CREDENTIALS" });

            if (!_hasher.Verify(req.Password, grpcResponse.PasswordHash))
                return Unauthorized(new { code = "INVALID_CREDENTIALS" });

            // Store in BOTH Session AND Cookie for maximum compatibility
            // Session for REST API calls
            var _ = HttpContext.Session.Id;
            HttpContext.Session.SetString(SessionKeys.UserId, grpcResponse.UserId);
            await HttpContext.Session.CommitAsync();

            // Cookie for Blazor components (survives navigation)
            Response.Cookies.Append(CookieKeys.UserId, grpcResponse.UserId, new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // Allow HTTP for localhost
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddHours(8),
                Path = "/"
            });

            _logger.LogInformation("=== LOGIN SUCCESS ===");
            _logger.LogInformation("UserId: {UserId}", grpcResponse.UserId);
            _logger.LogInformation("Session: {SessionId}", HttpContext.Session.Id);
            _logger.LogInformation("Cookie: Set");
            _logger.LogInformation("====================");

            return Ok(new LoginResponse 
            { 
                UserId = Guid.Parse(grpcResponse.UserId) 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login Error");
            return StatusCode(500, new { code = "SERVER_ERROR" });
        }
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        Response.Cookies.Delete(CookieKeys.UserId);
        return Ok(new { success = true });
    }
}
