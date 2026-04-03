using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AIHotkey;

internal static class ClipboardWorkflow
{
    public static async Task<string?> CaptureSelectionAsync(AppSettings settings, CancellationToken cancellationToken)
    {
        var previous = TryGetClipboardText();
        if (!string.IsNullOrWhiteSpace(previous))
        {
            return previous;
        }

        try
        {
            Clipboard.Clear();
            SendKeys.SendWait(settings.CopyShortcut);

            var start = DateTime.UtcNow;
            while ((DateTime.UtcNow - start).TotalMilliseconds < 1000)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(50, cancellationToken);

                var text = TryGetClipboardText();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }

            return null;
        }
        finally
        {
            RestoreClipboard(previous);
        }
    }

    public static async Task PasteReplacementAsync(string text, AppSettings settings, CancellationToken cancellationToken)
    {
        var previous = TryGetClipboardText();
        try
        {
            Clipboard.SetText(text);
            await Task.Delay(50, cancellationToken);
            SendKeys.SendWait(settings.PasteShortcut);
        }
        finally
        {
            await Task.Delay(100, cancellationToken);
            RestoreClipboard(previous);
        }
    }

    private static string? TryGetClipboardText()
    {
        try
        {
            return Clipboard.ContainsText() ? Clipboard.GetText() : null;
        }
        catch (ExternalException)
        {
            return null;
        }
    }

    private static void RestoreClipboard(string? previous)
    {
        try
        {
            if (string.IsNullOrEmpty(previous))
            {
                Clipboard.Clear();
            }
            else
            {
                Clipboard.SetText(previous);
            }
        }
        catch (ExternalException)
        {
        }
    }
}
