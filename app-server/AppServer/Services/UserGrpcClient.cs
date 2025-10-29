using Grpc.Net.Client;
using SemesterProjekt.Proto.User;

namespace AppServer.Services;

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

    public void Dispose()
    {
        _channel?.Dispose();
    }
}