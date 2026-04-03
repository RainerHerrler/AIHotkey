using System.Net.Http.Json;
using System.Text.Json;

namespace AIHotkey;

internal sealed class OllamaClient(HttpClient httpClient, AppSettings settings) : IRewriteClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<string> RewriteAsync(string selectedText, CancellationToken cancellationToken)
    {
        var prompt = string.Join(
            "\n",
            [
                "Rewrite the following selected text.",
                "Keep the same language unless the text explicitly asks for another language.",
                "Return only the rewritten text without explanations.",
                string.Empty,
                selectedText
            ]);

        return await SendGenerateRequestAsync(prompt, settings.SystemPrompt, settings.Temperature, cancellationToken);
    }

    public async Task<string> TestConnectionAsync(CancellationToken cancellationToken)
    {
        const string prompt = "Reply with exactly OK.";
        const string systemPrompt = "You are validating that the model is reachable. Reply with exactly OK.";
        return await SendGenerateRequestAsync(prompt, systemPrompt, 0, cancellationToken);
    }

    private async Task<string> SendGenerateRequestAsync(
        string prompt,
        string systemPrompt,
        double temperature,
        CancellationToken cancellationToken)
    {
        var payload = new
        {
            model = settings.Model,
            prompt,
            system = systemPrompt,
            stream = false,
            options = new
            {
                temperature
            }
        };

        using var response = await httpClient.PostAsJsonAsync(settings.OllamaUrl, payload, JsonOptions, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Ollama returned {(int)response.StatusCode}: {body}");
        }

        var result = JsonSerializer.Deserialize<OllamaResponse>(body, JsonOptions);
        if (string.IsNullOrWhiteSpace(result?.Response))
        {
            throw new InvalidOperationException("Ollama returned an empty response.");
        }

        return result.Response.Trim();
    }

    private sealed record OllamaResponse(string Response);
}
