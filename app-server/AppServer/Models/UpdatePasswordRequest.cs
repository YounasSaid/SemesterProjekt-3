using System.ComponentModel.DataAnnotations;

namespace AppServer.Models;

/// <summary>
/// Request model for updating user password
/// </summary>
public class UpdatePasswordRequest
{
    /// <summary>
    /// User's email (for verification)
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Current password
    /// </summary>
    [Required(ErrorMessage = "Current password is required")]
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// New password
    /// </summary>
    [Required(ErrorMessage = "New password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Confirm new password
    /// </summary>
    [Required(ErrorMessage = "Please confirm your new password")]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
