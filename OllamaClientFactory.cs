using System.Net.Http.Headers;

namespace AIHotkey;

internal static class OllamaClientFactory
{
    public static HttpClient CreateHttpClient(AppSettings settings)
    {
        var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(settings.HttpTimeoutSeconds)
        };
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return httpClient;
    }

    public static OllamaClient Create(AppSettings settings)
    {
        return new OllamaClient(CreateHttpClient(settings), settings);
    }

    public static HttpClient CreateOpenAiHttpClient(AppSettings settings)
    {
        var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(settings.HttpTimeoutSeconds)
        };
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return httpClient;
    }

    public static OpenAiClient CreateOpenAi(AppSettings settings)
    {
        return new OpenAiClient(CreateOpenAiHttpClient(settings), settings);
    }
}
