using System.Text.Encodings.Web;
using System.Text.Json;

namespace AIHotkey;

internal sealed class AppLogger
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false
    };

    private readonly string _logDirectory;
    private readonly string _logPath;

    public AppLogger()
    {
        _logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AIHotkey",
            "logs");
        Directory.CreateDirectory(_logDirectory);
        _logPath = Path.Combine(_logDirectory, "aihotkey.log");
    }

    public void Info(string message) => Write("INFO", message);

    public void Error(string message, Exception? exception = null)
    {
        var payload = exception is null
            ? message
            : $"{message}{Environment.NewLine}{exception}";
        Write("ERROR", payload);
    }

    public void LogRewrite(
        RewriteProvider provider,
        string sourceText,
        string? responseText,
        bool succeeded,
        string? errorMessage = null)
    {
        var entry = new
        {
            timestamp = DateTimeOffset.Now,
            provider = provider.ToString(),
            succeeded,
            sourceText,
            responseText,
            errorMessage
        };

        var json = JsonSerializer.Serialize(entry, JsonOptions) + Environment.NewLine;
        File.AppendAllText(GetDailyJsonLogPath(), json);
    }

    public string LogDirectory => _logDirectory;

    private string GetDailyJsonLogPath()
    {
        return Path.Combine(_logDirectory, $"aihotkey-{DateTime.Now:yyyy-MM-dd}.jsonl");
    }

    private void Write(string level, string message)
    {
        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}{Environment.NewLine}";
        File.AppendAllText(_logPath, line);
    }
}
