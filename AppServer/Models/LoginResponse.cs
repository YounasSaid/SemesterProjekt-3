namespace AppServer.Models;

/// <summary>
/// Response model for succesfuldt login.
/// Returnerer brugerens unikke ID.
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// ID'et på den indloggede bruger
    /// </summary>
    public Guid UserId { get; set; }
}
