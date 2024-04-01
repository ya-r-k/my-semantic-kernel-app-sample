using OllamaDriver.NET.Models;
using OllamaDriver.NET.ResponseModels;

namespace OllamaDriver.NET.Infrastructure;

public interface IAiAccessor
{
    Task<ChatResponseModel?> ExecuteChatCompletionAsync(IEnumerable<MessageInfo> messages, int? seed = null, float? temperature = null);

    IAsyncEnumerable<ChatResponseModel> ExecuteChatCompletionStreamingAsync(IEnumerable<MessageInfo> messages, int? seed = null, float? temperature = null);

    Task<EmbeddingResponseModel?> ExecuteEmbeddingGenerationAsync(string prompt, int? seed = null, float? temperature = null)
}
