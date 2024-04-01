using System.Text.Json;
using System.Text;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace OllamaDriver.NET.Extensions;

public static class HttpClientExtensions
{
    private static readonly JsonSerializerOptions serializerOptions;

    static HttpClientExtensions()
    {
        serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
    }

    public static async Task<T?> SendRequestAsync<T>(this HttpClient client, HttpMethod method, string url, object? requestBody = null)
    {
        var response = await client.SendRequestAsync(method, url, requestBody);

        if (response.IsSuccessStatusCode && response.Content is not null)
        {
            return await response.Content.ReadFromJsonAsync<T>(serializerOptions);
        }

        return default;
    }

    public static async Task<HttpResponseMessage> SendRequestAsync(this HttpClient client, HttpMethod method, string url, object? requestBody = null)
    {
        using var request = CreateHttpRequestMessage(method, url, requestBody);

        return await client.SendAsync(request);
    }

    public static async IAsyncEnumerable<T> SendStreamingRequestAsync<T>(this HttpClient client, HttpMethod method, string url, object? requestBody = null)
    {
        using var response = await client.SendSreamingRequestAsync(HttpMethod.Post, "api/chat", requestBody);

        if (response.IsSuccessStatusCode && response.Content is not null)
        {
            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var json = await reader.ReadLineAsync();

                if (string.IsNullOrEmpty(json))
                {
                    continue;
                }

                yield return JsonSerializer.Deserialize<T>(json, serializerOptions);
            }
        }

        yield break;
    }

    public static async Task<HttpResponseMessage> SendSreamingRequestAsync(this HttpClient client, HttpMethod method, string url, object? requestBody = null)
    {
        using var request = CreateHttpRequestMessage(method, url, requestBody);

        // https://stackoverflow.com/questions/37067355/c-how-to-post-async-request-and-get-stream-with-httpclient
        return await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
    }

    private static HttpRequestMessage CreateHttpRequestMessage(HttpMethod method, string url, object? requestBody = null)
    {
        var request = new HttpRequestMessage(method, url);

        if (requestBody is not null)
        {
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody, serializerOptions), Encoding.UTF8, "application/json");
        }

        return request;
    }
}
