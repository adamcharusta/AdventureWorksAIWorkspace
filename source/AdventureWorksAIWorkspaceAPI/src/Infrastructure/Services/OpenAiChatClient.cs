using System.ClientModel;
using System.ClientModel.Primitives;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Ai;
using AdventureWorksAIWorkspaceAPI.Application.Common.Services;
using AdventureWorksAIWorkspaceAPI.Infrastructure.OpenAi;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace AdventureWorksAIWorkspaceAPI.Infrastructure.Services;

/// <summary>
/// Sends chat completions to the OpenAI API using the official OpenAI .NET SDK over an
/// <see cref="HttpClient"/> supplied by <see cref="IHttpClientFactory"/>.
/// </summary>
public sealed class OpenAiChatClient : IAiChatClient
{
    private readonly ChatClient _chatClient;

    public OpenAiChatClient(HttpClient httpClient, IOptions<OpenAiOptions> options)
    {
        Guard.Against.Null(httpClient);
        Guard.Against.Null(options);

        OpenAiOptions value = options.Value;
        Guard.Against.NullOrWhiteSpace(value.ApiKey, message: "OpenAI API key is not configured.");
        Guard.Against.NullOrWhiteSpace(value.Model, message: "OpenAI model is not configured.");

        var clientOptions = new OpenAIClientOptions
        {
            Transport = new HttpClientPipelineTransport(httpClient)
        };

        if (!string.IsNullOrWhiteSpace(value.BaseUrl))
        {
            clientOptions.Endpoint = new Uri(value.BaseUrl);
        }

        _chatClient = new ChatClient(value.Model, new ApiKeyCredential(value.ApiKey), clientOptions);
    }

    public async Task<AiChatResult> CompleteAsync(
        IReadOnlyList<AiChatMessage> messages,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(messages);

        List<ChatMessage> chatMessages = messages.Select(ToChatMessage).ToList();

        ClientResult<ChatCompletion> result =
            await _chatClient.CompleteChatAsync(chatMessages, cancellationToken: cancellationToken);

        ChatCompletion completion = result.Value;
        string content = completion.Content.Count > 0 ? completion.Content[0].Text : string.Empty;

        return new AiChatResult(
            content,
            completion.Usage?.InputTokenCount,
            completion.Usage?.OutputTokenCount);
    }

    private static ChatMessage ToChatMessage(AiChatMessage message) => message.Role switch
    {
        AiChatRole.System => new SystemChatMessage(message.Content),
        AiChatRole.Assistant => new AssistantChatMessage(message.Content),
        _ => new UserChatMessage(message.Content)
    };
}
