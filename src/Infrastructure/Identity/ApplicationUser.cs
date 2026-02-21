using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure;

public class ApplicationUser : IdentityUser<Guid>
{
    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }

public string? DisplayName
{
    get
    {
        if (!string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName))
        {
            return $"{FirstName} {LastName}".Trim();
        }

        if (!string.IsNullOrWhiteSpace(FirstName))
        {
            return FirstName;
        }

        if (!string.IsNullOrWhiteSpace(LastName))
        {
            return LastName;
        }

        return Email ?? UserName;
    }
}
}
