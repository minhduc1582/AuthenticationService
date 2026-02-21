using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AuthenticationAPI.Security;

public static class AntiforgeryValidationExtensions
{
    public static async Task<bool> TryValidateRequestAsync(this IAntiforgery antiforgery, HttpContext context, ILogger logger)
    {
        try
        {
            await antiforgery.ValidateRequestAsync(context);
            return true;
        }
        catch (AntiforgeryValidationException ex)
        {
            logger.LogWarning(ex, "Antiforgery validation failed for {Path}", context.Request.Path);
            return false;
        }
    }
}
