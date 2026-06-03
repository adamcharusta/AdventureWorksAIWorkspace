using Microsoft.AspNetCore.Identity;

namespace AdventureWorksAIWorkspace.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public bool IsFirstLogin { get; set; }
}
