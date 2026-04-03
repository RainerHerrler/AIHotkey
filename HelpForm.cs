using System.Drawing;
using System.Windows.Forms;

namespace AIHotkey;

internal sealed class HelpForm : Form
{
    public HelpForm(AppSettings settings, string settingsPath, string logDirectory)
    {
        Text = "AIHotkey Hilfe";
        Width = 760;
        Height = 620;
        MinimumSize = new Size(680, 520);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = false;

        var closeButton = new Button
        {
            Text = "Schliessen",
            DialogResult = DialogResult.OK,
            Dock = DockStyle.Right,
            AutoSize = true,
            Padding = new Padding(14, 8, 14, 8)
        };

        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 62,
            Padding = new Padding(18, 10, 18, 14)
        };
        footer.Controls.Add(closeButton);

        var content = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            BackColor = SystemColors.Window,
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
            DetectUrls = false,
            Text = BuildHelpText(settings, settingsPath, logDirectory)
        };

        Controls.Add(content);
        Controls.Add(footer);

        AcceptButton = closeButton;
    }

    private static string BuildHelpText(AppSettings settings, string settingsPath, string logDirectory)
    {
        return string.Join(
            Environment.NewLine,
            [
                "AIHotkey Hilfe",
                "",
                "Hotkeys",
                $"{settings.HotkeyLabel}: markierten Text ueber Ollama umschreiben",
                $"{settings.OpenAiHotkeyLabel}: markierten Text ueber OpenAI umschreiben",
                "",
                "Empfohlener Ablauf in New Outlook",
                "1. Text markieren",
                "2. Manuell Ctrl+C druecken",
                $"3. Danach {settings.HotkeyLabel} oder {settings.OpenAiHotkeyLabel} druecken",
                "",
                "Wichtige Hinweise",
                "- Die App arbeitet ueber Zwischenablage sowie Ctrl+C und Ctrl+V.",
                "- In New Outlook ist manuelles Ctrl+C meist deutlich zuverlaessiger als automatisches Kopieren.",
                "- OpenAI benoetigt einen API-Key auf platform.openai.com; ChatGPT Pro allein reicht nicht.",
                "",
                "Dateien",
                $"Settings: {settingsPath}",
                $"Logs: {logDirectory}",
                "",
                "Logs",
                "- Normales Log: aihotkey.log",
                "- Tageslog mit Request und Response als JSONL: aihotkey-YYYY-MM-DD.jsonl",
                "",
                "Tray-Menue",
                "- Rewrite via Ollama",
                "- Rewrite via OpenAI",
                "- Settings",
                "- Open log folder",
                "- Help",
                "- Exit"
            ]);
    }
}
