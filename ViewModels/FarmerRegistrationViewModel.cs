using System.ComponentModel.DataAnnotations;

namespace AgriEnergyConnect.ViewModels;

/// <summary>
/// Captures the details an employee provides when onboarding a new farmer.
/// An Identity account and a <see cref="Models.Farmer"/> profile are created from this.
/// </summary>
public class FarmerRegistrationViewModel
{
    [Required(ErrorMessage = "Please enter the farmer's full name.")]
    [StringLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter a contact number.")]
    [Phone(ErrorMessage = "Please enter a valid contact number.")]
    [Display(Name = "Contact Number")]
    public string ContactNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter an address.")]
    [StringLength(200)]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter an email address.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter a password.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least {2} characters and include an uppercase letter, a lowercase letter, a digit and a symbol.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
