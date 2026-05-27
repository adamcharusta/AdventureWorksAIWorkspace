using Microsoft.AspNetCore.Identity;

namespace AdventureWorksAIWorkspaceAPI.Infrastructure.Identity;

public class ApplicationRole : IdentityRole<string>
{
    public ApplicationRole()
    {
        Id = Guid.NewGuid().ToString();
    }
}
