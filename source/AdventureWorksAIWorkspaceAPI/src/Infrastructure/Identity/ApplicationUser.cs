using Microsoft.AspNetCore.Identity;

namespace AdventureWorksAIWorkspaceAPI.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public bool IsFirstLogin { get; set; }
}
