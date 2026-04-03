namespace AIHotkey;

internal sealed class AppSettings
{
    public string OllamaUrl { get; set; } = "http://ollama.local/api/generate";
    public string Model { get; set; } = "gpt-oss:20b";
    public string OpenAiResponsesUrl { get; set; } = "https://api.openai.com/v1/responses";
    public string OpenAiModel { get; set; } = "gpt-5-mini";
    public string OpenAiApiKey { get; set; } = string.Empty;
    public string SystemPrompt { get; set; } =
        "You rewrite selected text in a clear, professional tone. Preserve the original language unless explicitly asked otherwise. Output only the rewritten text.";
    public double Temperature { get; set; } = 0.2;
    public string HotkeyLabel { get; set; } = "Ctrl+Shift+L";
    public string OpenAiHotkeyLabel { get; set; } = "Ctrl+Shift+O";
    public string CopyShortcut { get; set; } = "^c";
    public string PasteShortcut { get; set; } = "^v";
    public int HttpTimeoutSeconds { get; set; } = 120;
}
