using System.ComponentModel.DataAnnotations;

namespace AppServer.Models;

public class LoginRequest
{
    [Required, EmailAddress] public string SchoolEmail { get; set; } = "";
    [Required, MinLength(8)] public string Password { get; set; } = "";
}

public class LoginResponse
{
    public Guid UserId { get; set; }
}