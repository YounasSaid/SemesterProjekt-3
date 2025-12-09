using System.ComponentModel.DataAnnotations;

namespace AppServer.Models;

/// <summary>
/// Request model til opdatering af bruger password.
/// Validerer nuværende password og bekræfter nyt password.
/// </summary>
public class UpdatePasswordRequest
{
    /// <summary>
    /// Brugerens email (til verificering)
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Nuværende password
    /// </summary>
    [Required(ErrorMessage = "Current password is required")]
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// Nyt password
    /// </summary>
    [Required(ErrorMessage = "New password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Bekræft nyt password
    /// </summary>
    [Required(ErrorMessage = "Please confirm your new password")]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
