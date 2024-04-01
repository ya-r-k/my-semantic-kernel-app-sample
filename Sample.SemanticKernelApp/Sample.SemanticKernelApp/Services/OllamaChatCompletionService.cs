using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using OllamaDriver.NET.Infrastructure;
using OllamaDriver.NET.Models;

namespace Sample.SemanticKernelApp.Services;

public class OllamaChatCompletionService : IChatCompletionService
{
    private readonly IAiAccessor aiAccessor;

    public OllamaChatCompletionService(IAiAccessor aiAccessor)
    {
        this.aiAccessor = aiAccessor;
    }

    public IReadOnlyDictionary<string, object> Attributes => throw new NotImplementedException();

    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings executionSettings = null, Kernel kernel = null, CancellationToken cancellationToken = default)
    {
        var messages = chatHistory.Select(ch => new MessageInfo
        {
            Role = ch.Role.Label,
            Content = ch.Content ?? string.Empty,
        });

        var response = await aiAccessor.ExecuteChatCompletionAsync(messages);

        if (response is null || response.Message is null)
        {
            return [];
        }

        var role = new AuthorRole(response.Message.Role);

        return new List<ChatMessageContent>
        {
            new(role, response.Message.Content, response.Model),
        }.AsReadOnly();
    }

    public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings executionSettings = null, Kernel kernel = null, CancellationToken cancellationToken = default)
    {
        var messages = chatHistory.Select(ch => new MessageInfo
        {
            Role = ch.Role.Label,
            Content = ch.Content ?? string.Empty,
        });

        var responses = aiAccessor.ExecuteChatCompletionStreamingAsync(messages);

        await foreach (var response in responses)
        {
            AuthorRole? role = response.Message is null || string.IsNullOrEmpty(response.Message.Role)
                ? default
                : new AuthorRole(response.Message.Role);
            var messageContent = response.Message is null
                ? string.Empty
                : response.Message.Content;

            yield return new StreamingChatMessageContent(role, messageContent, modelId: response.Model);
        }

        yield break;
    }
}
