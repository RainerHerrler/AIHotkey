using System.Drawing;
using System.Windows.Forms;

namespace AIHotkey;

internal sealed class SettingsForm : Form
{
    private static readonly Color AppBackground = Color.FromArgb(243, 247, 250);
    private static readonly Color CardBackground = Color.White;
    private static readonly Color BorderColor = Color.FromArgb(214, 223, 231);
    private static readonly Color TitleColor = Color.FromArgb(25, 40, 56);
    private static readonly Color MutedColor = Color.FromArgb(98, 114, 130);
    private static readonly Color AccentColor = Color.FromArgb(14, 116, 144);
    private static readonly Color SuccessColor = Color.FromArgb(24, 102, 40);
    private static readonly Color ErrorColor = Color.FromArgb(156, 26, 26);
    private static readonly Color InfoColor = Color.FromArgb(59, 89, 152);

    private readonly TextBox _ollamaUrlTextBox = new();
    private readonly TextBox _modelTextBox = new();
    private readonly TextBox _ollamaHotkeyTextBox = new() { ReadOnly = true, TabStop = false };
    private readonly TextBox _openAiUrlTextBox = new();
    private readonly TextBox _openAiModelTextBox = new();
    private readonly TextBox _openAiApiKeyTextBox = new() { UseSystemPasswordChar = true };
    private readonly TextBox _openAiHotkeyTextBox = new() { ReadOnly = true, TabStop = false };
    private readonly NumericUpDown _temperatureUpDown = new()
    {
        DecimalPlaces = 1,
        Increment = 0.1M,
        Minimum = 0,
        Maximum = 2,
        Width = 120
    };
    private readonly NumericUpDown _timeoutUpDown = new()
    {
        Minimum = 5,
        Maximum = 600,
        Width = 120
    };
    private readonly TextBox _systemPromptTextBox = new()
    {
        Multiline = true,
        ScrollBars = ScrollBars.Vertical
    };
    private readonly Label _ollamaTestStatusLabel = new()
    {
        AutoSize = false,
        Text = "Noch kein Verbindungstest ausgefuehrt.",
        ForeColor = MutedColor,
        TextAlign = ContentAlignment.MiddleLeft,
        MinimumSize = new Size(260, 42),
        Margin = new Padding(12, 0, 0, 0),
        Dock = DockStyle.Fill
    };
    private readonly Label _openAiTestStatusLabel = new()
    {
        AutoSize = false,
        Text = "Noch kein OpenAI-Test ausgefuehrt.",
        ForeColor = MutedColor,
        TextAlign = ContentAlignment.MiddleLeft,
        MinimumSize = new Size(260, 42),
        Margin = new Padding(12, 0, 0, 0),
        Dock = DockStyle.Fill
    };
    private readonly Button _ollamaTestButton = new() { Text = "Ollama testen", AutoSize = true };
    private readonly Button _openAiTestButton = new() { Text = "OpenAI testen", AutoSize = true };
    private readonly Button _saveButton = new() { Text = "Speichern", DialogResult = DialogResult.OK, AutoSize = true };
    private readonly Button _cancelButton = new() { Text = "Abbrechen", DialogResult = DialogResult.Cancel, AutoSize = true };

    public SettingsForm(AppSettings settings)
    {
        Text = "AIHotkey Settings";
        Width = 1100;
        Height = 860;
        MinimumSize = new Size(980, 760);
        StartPosition = FormStartPosition.CenterScreen;
        MinimizeBox = false;
        MaximizeBox = true;
        FormBorderStyle = FormBorderStyle.Sizable;
        BackColor = AppBackground;
        Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        AutoScaleMode = AutoScaleMode.Dpi;

        ConfigureInput(_ollamaUrlTextBox);
        ConfigureInput(_modelTextBox);
        ConfigureInput(_ollamaHotkeyTextBox, readOnly: true);
        ConfigureInput(_openAiUrlTextBox);
        ConfigureInput(_openAiModelTextBox);
        ConfigureInput(_openAiApiKeyTextBox);
        ConfigureInput(_openAiHotkeyTextBox, readOnly: true);
        ConfigureInput(_systemPromptTextBox, multiline: true);
        ConfigureNumeric(_temperatureUpDown);
        ConfigureNumeric(_timeoutUpDown);
        ConfigurePrimaryButton(_saveButton);
        ConfigureSecondaryButton(_cancelButton);
        ConfigureSecondaryButton(_ollamaTestButton);
        ConfigureSecondaryButton(_openAiTestButton);
        _ollamaTestButton.MinimumSize = new Size(170, 42);
        _openAiTestButton.MinimumSize = new Size(170, 42);

        _ollamaUrlTextBox.Text = settings.OllamaUrl;
        _modelTextBox.Text = settings.Model;
        _ollamaHotkeyTextBox.Text = settings.HotkeyLabel;
        _openAiUrlTextBox.Text = settings.OpenAiResponsesUrl;
        _openAiModelTextBox.Text = settings.OpenAiModel;
        _openAiApiKeyTextBox.Text = settings.OpenAiApiKey;
        _openAiHotkeyTextBox.Text = settings.OpenAiHotkeyLabel;
        _temperatureUpDown.Value = (decimal)settings.Temperature;
        _timeoutUpDown.Value = settings.HttpTimeoutSeconds;
        _systemPromptTextBox.Text = settings.SystemPrompt;

        _ollamaTestButton.Click += async (_, _) => await TestOllamaConnectionAsync();
        _openAiTestButton.Click += async (_, _) => await TestOpenAiConnectionAsync();
        _saveButton.Click += (_, _) => ValidateBeforeSave();

        var shell = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(22, 20, 22, 20),
            BackColor = AppBackground
        };
        shell.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 1F));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 1F));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 1F));
        shell.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        shell.Controls.Add(BuildHeader(), 0, 0);
        shell.Controls.Add(BuildOllamaCard(), 0, 1);
        shell.Controls.Add(BuildModelCard(), 0, 2);
        shell.Controls.Add(BuildOpenAiCard(), 0, 3);
        shell.Controls.Add(BuildFooter(), 0, 4);

        Controls.Add(shell);

        AcceptButton = _saveButton;
        CancelButton = _cancelButton;
    }

    public AppSettings BuildSettings(AppSettings current) => BuildDraftSettings(current);

    private Control BuildHeader()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Height = 92,
            Margin = new Padding(0, 0, 0, 12),
            Padding = new Padding(0, 0, 0, 12),
            BackColor = AppBackground
        };

        var title = new Label
        {
            Dock = DockStyle.Top,
            Height = 38,
            Font = new Font(Font, FontStyle.Bold),
            ForeColor = TitleColor,
            Text = "Einstellungen"
        };

        var subtitle = new Label
        {
            Dock = DockStyle.Fill,
            ForeColor = MutedColor,
            Text = "Zwei Rewrite-Ziele mit separaten Hotkeys, gemeinsamem Systemprompt und JSONL-Tageslogs."
        };

        panel.Controls.Add(subtitle);
        panel.Controls.Add(title);
        return panel;
    }

    private Control BuildOllamaCard()
    {
        var layout = CreateCardLayout();
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 86));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 94));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        AddSectionTitle(layout, "Ollama", 0);
        layout.Controls.Add(BuildLabel("Hotkey"), 0, 1);
        layout.Controls.Add(_ollamaHotkeyTextBox, 1, 1);
        layout.Controls.Add(BuildLabel("Ollama-URL"), 0, 2);
        layout.Controls.Add(_ollamaUrlTextBox, 1, 2);
        layout.Controls.Add(BuildLabel("Modell"), 0, 3);
        layout.Controls.Add(_modelTextBox, 1, 3);
        layout.Controls.Add(BuildLabel("HTTP-Timeout"), 0, 4);
        layout.Controls.Add(BuildTimeoutAndTestRow(_timeoutUpDown, "Sekunden", _ollamaTestButton, _ollamaTestStatusLabel), 1, 4);

        return BuildCard(layout);
    }

    private Control BuildModelCard()
    {
        var layout = CreateCardLayout();
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 86));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        AddSectionTitle(layout, "Modellkonfiguration", 0);
        layout.Controls.Add(BuildLabel("Temperatur"), 0, 1);
        layout.Controls.Add(BuildInlineField(_temperatureUpDown, "0.0 bis 2.0"), 1, 1);
        layout.Controls.Add(BuildLabel("Systemprompt"), 0, 2);
        layout.Controls.Add(_systemPromptTextBox, 1, 2);

        return BuildCard(layout);
    }

    private Control BuildOpenAiCard()
    {
        var layout = CreateCardLayout();
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 86));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        AddSectionTitle(layout, "OpenAI", 0);
        layout.Controls.Add(BuildLabel("Hotkey"), 0, 1);
        layout.Controls.Add(_openAiHotkeyTextBox, 1, 1);
        layout.Controls.Add(BuildLabel("Responses-URL"), 0, 2);
        layout.Controls.Add(_openAiUrlTextBox, 1, 2);
        layout.Controls.Add(BuildLabel("OpenAI-Modell"), 0, 3);
        layout.Controls.Add(_openAiModelTextBox, 1, 3);
        layout.Controls.Add(BuildLabel("API-Key"), 0, 4);
        layout.Controls.Add(_openAiApiKeyTextBox, 1, 4);
        layout.Controls.Add(BuildLabel("Verbindungstest"), 0, 5);
        layout.Controls.Add(BuildTestRow(_openAiTestButton, _openAiTestStatusLabel), 1, 5);

        return BuildCard(layout);
    }

    private Control BuildFooter()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Height = 86,
            Padding = new Padding(0, 10, 0, 0),
            Margin = new Padding(0),
            BackColor = AppBackground
        };

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            AutoSize = true
        };
        buttons.Controls.Add(_saveButton);
        buttons.Controls.Add(_cancelButton);

        panel.Controls.Add(buttons);
        return panel;
    }

    private async Task TestOllamaConnectionAsync()
    {
        AppSettings draftSettings;
        try
        {
            draftSettings = BuildDraftSettings(null);
        }
        catch (InvalidOperationException ex)
        {
            SetStatus(_ollamaTestStatusLabel, ex.Message, ErrorColor);
            return;
        }

        SetBusyState(true);
        SetStatus(_ollamaTestStatusLabel, "Teste Ollama-Verbindung ...", InfoColor);

        try
        {
            using var httpClient = OllamaClientFactory.CreateHttpClient(draftSettings);
            var client = new OllamaClient(httpClient, draftSettings);
            var response = await client.TestConnectionAsync(CancellationToken.None);
            SetStatus(_ollamaTestStatusLabel, $"Verbindung erfolgreich. Modellantwort: {response}", SuccessColor);
        }
        catch (Exception ex)
        {
            SetStatus(_ollamaTestStatusLabel, ex.Message, ErrorColor);
        }
        finally
        {
            SetBusyState(false);
        }
    }

    private async Task TestOpenAiConnectionAsync()
    {
        AppSettings draftSettings;
        try
        {
            draftSettings = BuildDraftSettings(null);
        }
        catch (InvalidOperationException ex)
        {
            SetStatus(_openAiTestStatusLabel, ex.Message, ErrorColor);
            return;
        }

        SetBusyState(true);
        SetStatus(_openAiTestStatusLabel, "Teste OpenAI-Verbindung ...", InfoColor);

        try
        {
            using var httpClient = OllamaClientFactory.CreateOpenAiHttpClient(draftSettings);
            var client = new OpenAiClient(httpClient, draftSettings);
            var response = await client.TestConnectionAsync(CancellationToken.None);
            SetStatus(_openAiTestStatusLabel, $"Verbindung erfolgreich. Modellantwort: {response}", SuccessColor);
        }
        catch (Exception ex)
        {
            SetStatus(_openAiTestStatusLabel, ex.Message, ErrorColor);
        }
        finally
        {
            SetBusyState(false);
        }
    }

    private void ValidateBeforeSave()
    {
        try
        {
            BuildDraftSettings(null);
        }
        catch (InvalidOperationException ex)
        {
            DialogResult = DialogResult.None;
            SetStatus(_ollamaTestStatusLabel, ex.Message, ErrorColor);
        }
    }

    private AppSettings BuildDraftSettings(AppSettings? current)
    {
        var ollamaUrl = _ollamaUrlTextBox.Text.Trim();
        var model = _modelTextBox.Text.Trim();
        var openAiUrl = _openAiUrlTextBox.Text.Trim();
        var openAiModel = _openAiModelTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(ollamaUrl))
        {
            throw new InvalidOperationException("Bitte eine Ollama-URL eintragen.");
        }

        if (!Uri.TryCreate(ollamaUrl, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException("Die Ollama-URL ist nicht gueltig.");
        }

        if (string.IsNullOrWhiteSpace(model))
        {
            throw new InvalidOperationException("Bitte ein Ollama-Modell angeben.");
        }

        if (string.IsNullOrWhiteSpace(openAiUrl))
        {
            throw new InvalidOperationException("Bitte eine OpenAI-Responses-URL eintragen.");
        }

        if (!Uri.TryCreate(openAiUrl, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException("Die OpenAI-Responses-URL ist nicht gueltig.");
        }

        if (string.IsNullOrWhiteSpace(openAiModel))
        {
            throw new InvalidOperationException("Bitte ein OpenAI-Modell angeben.");
        }

        return new AppSettings
        {
            OllamaUrl = ollamaUrl,
            Model = model,
            OpenAiResponsesUrl = openAiUrl,
            OpenAiModel = openAiModel,
            OpenAiApiKey = _openAiApiKeyTextBox.Text.Trim(),
            Temperature = (double)_temperatureUpDown.Value,
            HttpTimeoutSeconds = (int)_timeoutUpDown.Value,
            SystemPrompt = _systemPromptTextBox.Text.Trim(),
            HotkeyLabel = current?.HotkeyLabel ?? _ollamaHotkeyTextBox.Text,
            OpenAiHotkeyLabel = current?.OpenAiHotkeyLabel ?? _openAiHotkeyTextBox.Text,
            CopyShortcut = current?.CopyShortcut ?? "^c",
            PasteShortcut = current?.PasteShortcut ?? "^v"
        };
    }

    private void SetBusyState(bool isBusy)
    {
        _ollamaTestButton.Enabled = !isBusy;
        _openAiTestButton.Enabled = !isBusy;
        _saveButton.Enabled = !isBusy;
        _cancelButton.Enabled = !isBusy;
        UseWaitCursor = isBusy;
    }

    private static void SetStatus(Label label, string message, Color color)
    {
        label.Text = message;
        label.ForeColor = color;
    }

    private static Control BuildTimeoutAndTestRow(Control control, string suffix, Button button, Label statusLabel)
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            Margin = Padding.Empty
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        panel.Controls.Add(BuildInlineField(control, suffix), 0, 0);
        panel.Controls.Add(button, 1, 0);
        panel.Controls.Add(statusLabel, 2, 0);
        return panel;
    }

    private static Control BuildTestRow(Button button, Label statusLabel)
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Margin = Padding.Empty
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        panel.Controls.Add(button, 0, 0);
        panel.Controls.Add(statusLabel, 1, 0);
        return panel;
    }

    private static TableLayoutPanel CreateCardLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 0,
            Padding = new Padding(24, 20, 24, 24),
            BackColor = CardBackground,
            Margin = new Padding(0, 0, 0, 18)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        return layout;
    }

    private static Panel BuildCard(Control content)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(1),
            BackColor = BorderColor,
            Margin = new Padding(0, 0, 0, 18)
        };

        var inner = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = CardBackground
        };
        inner.Controls.Add(content);
        panel.Controls.Add(inner);
        return panel;
    }

    private static void AddSectionTitle(TableLayoutPanel layout, string title, int row)
    {
        var titleControl = BuildSectionTitle(title);
        layout.Controls.Add(titleControl, 0, row);
        layout.SetColumnSpan(titleControl, 2);
    }

    private static Control BuildSectionTitle(string title)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            MinimumSize = new Size(0, 52),
            Height = 52,
            Margin = new Padding(0, 0, 0, 8)
        };

        var titleLabel = new Label
        {
            Dock = DockStyle.Fill,
            MinimumSize = new Size(0, 40),
            Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = TitleColor,
            TextAlign = ContentAlignment.MiddleLeft,
            Text = title
        };
        panel.Controls.Add(titleLabel);
        return panel;
    }

    private static Control BuildInlineField(Control control, string suffix)
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Left,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = Padding.Empty
        };
        panel.Controls.Add(control);
        panel.Controls.Add(new Label
        {
            AutoSize = true,
            ForeColor = MutedColor,
            Padding = new Padding(8, 10, 0, 0),
            Text = suffix
        });
        return panel;
    }

    private static Label BuildLabel(string text)
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ForeColor = TitleColor,
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
            Padding = new Padding(0, 10, 0, 0),
            Text = text
        };
    }

    private static void ConfigureInput(TextBox textBox, bool multiline = false, bool readOnly = false)
    {
        textBox.Dock = DockStyle.Fill;
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.Margin = Padding.Empty;
        textBox.BackColor = Color.White;
        textBox.ReadOnly = readOnly;
        if (multiline)
        {
            textBox.AcceptsReturn = true;
        }
    }

    private static void ConfigureNumeric(NumericUpDown numeric)
    {
        numeric.BorderStyle = BorderStyle.FixedSingle;
        numeric.Margin = Padding.Empty;
        numeric.BackColor = Color.White;
        numeric.TextAlign = HorizontalAlignment.Left;
    }

    private static void ConfigurePrimaryButton(Button button)
    {
        button.BackColor = AccentColor;
        button.ForeColor = Color.White;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.Padding = new Padding(16, 9, 16, 9);
        button.Margin = new Padding(12, 0, 0, 0);
        button.MinimumSize = new Size(120, 42);
    }

    private static void ConfigureSecondaryButton(Button button)
    {
        button.BackColor = Color.White;
        button.ForeColor = TitleColor;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = BorderColor;
        button.FlatAppearance.BorderSize = 1;
        button.Padding = new Padding(16, 9, 16, 9);
        button.Margin = new Padding(12, 0, 0, 0);
        button.MinimumSize = new Size(120, 42);
    }
}
