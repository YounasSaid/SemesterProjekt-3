using System.ComponentModel.DataAnnotations;

namespace AppServer.Models;

public class RegisterRequest
{
    [Required] public string FirstName { get; set; } = "";
    [Required] public string LastName  { get; set; } = "";
    [Required, EmailAddress] public string SchoolEmail { get; set; } = "";
    [Required, MinLength(8)] public string Password { get; set; } = "";
    [Required, Range(1,7)] public int Semester { get; set; } = 3;
}

