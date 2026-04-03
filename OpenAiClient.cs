using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIHotkey;

internal sealed class OpenAiClient(HttpClient httpClient, AppSettings settings) : IRewriteClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<string> RewriteAsync(string selectedText, CancellationToken cancellationToken)
    {
        var instructions = string.Join(
            "\n",
            [
                settings.SystemPrompt,
                "Rewrite the provided text.",
                "Keep the same language unless the text explicitly asks for another language.",
                "Return only the rewritten text without explanations."
            ]);

        return await SendRequestAsync(instructions, selectedText, cancellationToken);
    }

    public async Task<string> TestConnectionAsync(CancellationToken cancellationToken)
    {
        const string instructions = "You are validating API connectivity. Reply with exactly OK.";
        return await SendRequestAsync(instructions, "Reply with exactly OK.", cancellationToken);
    }

    private async Task<string> SendRequestAsync(string instructions, string input, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.OpenAiApiKey))
        {
            throw new InvalidOperationException("OpenAI API key is missing.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, settings.OpenAiResponsesUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.OpenAiApiKey);
        request.Content = JsonContent.Create(
            new
            {
                model = settings.OpenAiModel,
                instructions,
                input
            },
            options: JsonOptions);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"OpenAI returned {(int)response.StatusCode}: {body}");
        }

        var result = JsonSerializer.Deserialize<OpenAiResponse>(body, JsonOptions);
        var outputText = ExtractOutputText(result);
        if (string.IsNullOrWhiteSpace(outputText))
        {
            throw new InvalidOperationException($"OpenAI returned an empty response. Raw body: {body}");
        }

        return outputText;
    }

    private static string? ExtractOutputText(OpenAiResponse? response)
    {
        if (!string.IsNullOrWhiteSpace(response?.OutputText))
        {
            return response.OutputText.Trim();
        }

        if (response?.Output is null)
        {
            return null;
        }

        var textParts = response.Output
            .SelectMany(item => item.Content ?? [])
            .Where(content => string.Equals(content.Type, "output_text", StringComparison.OrdinalIgnoreCase))
            .Select(content => content.Text?.Trim())
            .Where(text => !string.IsNullOrWhiteSpace(text));

        var combined = string.Join(Environment.NewLine, textParts!);
        return string.IsNullOrWhiteSpace(combined) ? null : combined;
    }

    private sealed record OpenAiResponse(
        [property: JsonPropertyName("output_text")] string? OutputText,
        [property: JsonPropertyName("output")] OpenAiOutputItem[]? Output);

    private sealed record OpenAiOutputItem(
        [property: JsonPropertyName("content")] OpenAiContentItem[]? Content);

    private sealed record OpenAiContentItem(
        [property: JsonPropertyName("type")] string? Type,
        [property: JsonPropertyName("text")] string? Text);
}
