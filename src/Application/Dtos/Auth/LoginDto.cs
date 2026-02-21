using System.ComponentModel.DataAnnotations;

namespace Application;

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = default!;

    public bool RememberMe { get; set; }

    public string? RedirectUrl { get; set; }
}
