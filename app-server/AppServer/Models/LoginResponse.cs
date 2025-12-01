namespace AppServer.Models;

/// <summary>
/// Response model for successful login
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// The ID of the logged in user
    /// </summary>
    public Guid UserId { get; set; }
}
