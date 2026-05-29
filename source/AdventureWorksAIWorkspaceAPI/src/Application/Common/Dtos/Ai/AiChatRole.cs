namespace AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Ai;

/// <summary>
/// The author role of a single chat message exchanged with the AI model.
/// </summary>
public enum AiChatRole
{
    /// <summary>Instructions that set the model's behaviour and constraints.</summary>
    System,

    /// <summary>Input authored by the end user.</summary>
    User,

    /// <summary>A previous response authored by the model.</summary>
    Assistant
}
