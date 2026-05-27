namespace AdventureWorksAIWorkspaceAPI.Infrastructure.Identity;

public class InitialAdminOptions
{
    public const string SectionName = "Identity:InitialAdmin";

    public bool Enabled { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string TemplatePassword { get; set; } = string.Empty;
}
