namespace AdventureWorksAIWorkspace.Application.Common.Dtos.Ai;

/// <summary>
/// A single message in a chat exchange with the AI model.
/// </summary>
/// <param name="Role">The author role of the message.</param>
/// <param name="Content">The message text.</param>
public sealed record AiChatMessage(AiChatRole Role, string Content)
{
    /// <summary>Creates a system message that sets the model's behaviour.</summary>
    public static AiChatMessage System(string content) => new(AiChatRole.System, content);

    /// <summary>Creates a user message.</summary>
    public static AiChatMessage User(string content) => new(AiChatRole.User, content);

    /// <summary>Creates an assistant message representing a previous model response.</summary>
    public static AiChatMessage Assistant(string content) => new(AiChatRole.Assistant, content);
}
