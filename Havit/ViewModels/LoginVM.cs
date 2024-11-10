using System.ComponentModel.DataAnnotations;
using Havit.Models;

namespace Havit.ViewModels;

public class LoginVM : DataTrackerModel
{
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Display(Name = "Remember me?")] public bool RememberMe { get; set; } = false;
}