using OllamaDriver.NET.Extensions;
using OllamaDriver.NET.Models;
using OllamaDriver.NET.ResponseModels;
using OllamaDriver.NET.Settings;

namespace OllamaDriver.NET.Infrastructure;

public class OllamaAccessor : IAiAccessor
{
    private readonly HttpClient httpClient;

    private readonly OllamaConnectionConfigs configs;

    public OllamaAccessor(OllamaConnectionConfigs configs)
    {
        httpClient = new HttpClient
        {
            BaseAddress = new Uri(configs.BaseUrl),
        };

        this.configs = configs;
    }

    public OllamaAccessor(IHttpClientFactory httpClientFactory, OllamaConnectionConfigs configs)
    {
        httpClient = httpClientFactory.CreateClient(nameof(OllamaAccessor));

        this.configs = configs;
    }

    public Task<ChatResponseModel?> ExecuteChatCompletionAsync(IEnumerable<MessageInfo> messages, int? seed = null, float? temperature = null)
    {
        return ExecuteChatCompletionAsync(new ChatRequestModel
        {
            Model = configs.ModelName,
            Messages = messages,
            Options = new OptionsRequestModelPart
            {
                Seed = seed,
                Temperature = temperature,
            },
            Stream = false,
        });
    }

    public IAsyncEnumerable<ChatResponseModel> ExecuteChatCompletionStreamingAsync(IEnumerable<MessageInfo> messages, int? seed = null, float? temperature = null)
    {
        return ExecuteChatCompletionStreamingAsync(new ChatRequestModel
        {
            Model = configs.ModelName,
            Messages = messages,
            Options = new OptionsRequestModelPart
            {
                Seed = seed,
                Temperature = temperature,
            },
        });
    }

    public Task<EmbeddingResponseModel?> ExecuteEmbeddingGenerationAsync(string prompt, int? seed = null, float? temperature = null)
    {
        return ExecuteEmbeddingGenerationAsync(new EmbeddingsRequestModel
        {
            Model = configs.ModelName,
            Prompt = prompt,
            Options = new OptionsRequestModelPart
            {
                Seed = seed,
                Temperature = temperature,
            },
        });
    }

    private async Task<ChatResponseModel?> ExecuteChatCompletionAsync(ChatRequestModel model)
    {
        return await httpClient.SendRequestAsync<ChatResponseModel>(HttpMethod.Post, "api/chat", model);
    }

    private IAsyncEnumerable<ChatResponseModel> ExecuteChatCompletionStreamingAsync(ChatRequestModel model)
    {
        return httpClient.SendStreamingRequestAsync<ChatResponseModel>(HttpMethod.Post, "api/chat", model);
    }

    private async Task<EmbeddingResponseModel?> ExecuteEmbeddingGenerationAsync(EmbeddingsRequestModel model)
    {
        return await httpClient.SendRequestAsync<EmbeddingResponseModel>(HttpMethod.Post, "api/embeddings", model);
    }
}
