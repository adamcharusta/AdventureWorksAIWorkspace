using Microsoft.AspNetCore.Identity;

namespace AdventureWorksAIWorkspace.Infrastructure.Identity;

public class ApplicationRole : IdentityRole<string>
{
    public ApplicationRole()
    {
        Id = Guid.NewGuid().ToString();
    }
}
