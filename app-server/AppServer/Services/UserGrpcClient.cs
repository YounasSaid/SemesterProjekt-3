using Grpc.Net.Client;
using SemesterProjekt.Proto.User;

namespace AppServer.Services;

/// <summary>
/// gRPC client for user operations (UPDATED)
/// </summary>
public class UserGrpcClient : IDisposable
{
    private readonly GrpcChannel _channel;
    private readonly UserService.UserServiceClient _client;

    public UserGrpcClient(IConfiguration config)
    {
        var grpcAddress = config["GrpcSettings:DataServerAddress"] 
                         ?? "http://localhost:9090";
        
        _channel = GrpcChannel.ForAddress(grpcAddress);
        _client = new UserService.UserServiceClient(_channel);
    }

    public async Task<(bool success, string? userId, string? errorCode)> CreateUserAsync(
        string email, 
        string firstName, 
        string lastName, 
        string passwordHash, 
        int semester)
    {
        try
        {
            var request = new CreateUserRequest
            {
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PasswordHash = passwordHash,
                Semester = semester
            };

            var response = await _client.CreateUserAsync(request);
            return (true, response.UserId, null);
        }
        catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.AlreadyExists)
        {
            return (false, null, "EMAIL_TAKEN");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"gRPC Error: {ex.Message}");
            return (false, null, "INTERNAL_ERROR");
        }
    }

    public async Task<(bool found, string? userId, string? email, string? passwordHash, int semester)> GetUserByEmailAsync(string email)
    {
        try
        {
            var request = new GetUserByEmailRequest { Email = email };
            var response = await _client.GetUserByEmailAsync(request);

            if (response.Found)
            {
                return (true, response.UserId, response.Email, response.PasswordHash, response.Semester);
            }
            return (false, null, null, null, 0);
        }
        catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
        {
            return (false, null, null, null, 0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"gRPC Error: {ex.Message}");
            return (false, null, null, null, 0);
        }
    }

    // ========================
    // NY - Get User By ID
    // ========================
    /// <summary>
    /// Henter bruger via user ID (til profilside)
    /// </summary>
    public async Task<UserProfileDto?> GetUserByIdAsync(Guid userId)
    {
        try
        {
            var request = new GetUserByIdRequest { UserId = userId.ToString() };
            var response = await _client.GetUserByIdAsync(request);

            if (response.Found)
            {
                return new UserProfileDto
                {
                    UserId = Guid.Parse(response.UserId),
                    Email = response.Email,
                    FirstName = response.FirstName,
                    LastName = response.LastName,
                    Semester = response.Semester
                };
            }
            return null;
        }
        catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"gRPC Error in GetUserById: {ex.Message}");
            return null;
        }
    }

    // ========================
    // NY - Update Password
    // ========================
    /// <summary>
    /// Opdaterer brugerens password.
    /// VIGTIGT: AppServer skal have verificeret currentPassword FØRST!
    /// </summary>
    public async Task<(bool success, string? errorCode)> UpdatePasswordAsync(
        Guid userId, 
        string newPasswordHash)
    {
        try
        {
            var request = new UpdatePasswordRequest
            {
                UserId = userId.ToString(),
                NewPasswordHash = newPasswordHash
            };

            var response = await _client.UpdatePasswordAsync(request);
            
            if (response.Success)
            {
                return (true, null);
            }
            else
            {
                return (false, response.ErrorCode);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"gRPC Error in UpdatePassword: {ex.Message}");
            return (false, "INTERNAL_ERROR");
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
    }
}

// DTO for user profile
public class UserProfileDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int Semester { get; set; }
}
