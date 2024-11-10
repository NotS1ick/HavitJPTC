using System.ComponentModel.DataAnnotations;
using Havit.Models;

namespace Havit.ViewModels;

public class RegisterVM : DataTrackerModel
{
    [Required(ErrorMessage = "Username is required for registration.")]
    [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Username can only contain letters and numbers")]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
        MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required for registration.")]
    [DataType(DataType.Password)]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm the password.")]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm password")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "You must agree to the Terms of Service")]
    public bool AgreedToTos { get; set; }
}