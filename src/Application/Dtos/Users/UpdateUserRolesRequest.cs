using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Application;

public class UpdateUserRolesRequest
{
    [Required]
    public IReadOnlyCollection<string> Roles { get; set; } = Array.Empty<string>();
}
