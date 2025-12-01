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

    public AuthController(IPasswordHasher hasher)
    {
        _hasher = hasher;
        
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
            Console.WriteLine($"Register Error: {ex.Message}");
            return StatusCode(500, new { code = "SERVER_ERROR" });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
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

            // Set session
            HttpContext.Session.SetString(SessionKeys.UserId, grpcResponse.UserId);
            await HttpContext.Session.CommitAsync();

            return Ok(new LoginResponse 
            { 
                UserId = Guid.Parse(grpcResponse.UserId) 
            });
        }
        catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
        {
            return Unauthorized(new { code = "INVALID_CREDENTIALS" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login Error: {ex.Message}");
            return StatusCode(500, new { code = "SERVER_ERROR" });
        }
    }
}
