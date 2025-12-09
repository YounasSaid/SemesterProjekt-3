using System;
using System.ComponentModel.DataAnnotations;

namespace AppServer.Models;

/// <summary>
/// Request model til bruger login.
/// Validerer skole email og password længde.
/// </summary>
public class LoginRequest
{
    [Required, EmailAddress] public string SchoolEmail { get; set; } = "";
    [Required, MinLength(8)] public string Password { get; set; } = "";
}
