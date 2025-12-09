using System;

namespace AppServer.Models;

/// <summary>
/// Response model for succesfuld bruger registrering.
/// Returnerer den nyoprettede brugers ID.
/// </summary>
public class RegisterResponse
{
    public Guid UserId { get; set; }
}
