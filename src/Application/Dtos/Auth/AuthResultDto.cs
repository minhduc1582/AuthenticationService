using System;
using System.Collections.Generic;

namespace Application;

public class AuthResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? RedirectUrl { get; set; }
    public CurrentUserDto? User { get; set; }
    public IEnumerable<string> Errors { get; set; } = Array.Empty<string>();

    public static AuthResultDto Ok(string message, CurrentUserDto? user = null, string? redirect = null) =>
        new()
        {
            Success = true,
            Message = message,
            User = user,
            RedirectUrl = redirect
        };

    public static AuthResultDto Fail(string message, IEnumerable<string>? errors = null) =>
        new()
        {
            Success = false,
            Message = message,
            Errors = errors ?? Array.Empty<string>()
        };
}
