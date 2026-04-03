using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace AIHotkey;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly object _requestSync = new();
    private readonly NotifyIcon _notifyIcon;
    private readonly HotkeyWindow _hotkeyWindow;
    private readonly SettingsStore _settingsStore;
    private readonly AppLogger _logger;
    private readonly Icon _trayIcon;
    private AppSettings _settings;
    private OllamaClient _ollamaClient;
    private OpenAiClient _openAiClient;
    private CancellationTokenSource? _activeRequestCts;
    private RewriteProvider? _queuedProvider;
    private int _isBusy;

    public TrayApplicationContext()
    {
        _settingsStore = new SettingsStore();
        _logger = new AppLogger();
        _settings = _settingsStore.Load();
        _ollamaClient = OllamaClientFactory.Create(_settings);
        _openAiClient = OllamaClientFactory.CreateOpenAi(_settings);
        _trayIcon = TrayIconFactory.CreateKiIcon();

        _notifyIcon = new NotifyIcon
        {
            Text = "AIHotkey",
            Icon = _trayIcon,
            Visible = true,
            ContextMenuStrip = BuildMenu()
        };

        _hotkeyWindow = new HotkeyWindow();
        if (_hotkeyWindow.IsOllamaRegistered)
        {
            _hotkeyWindow.OllamaHotkeyPressed += async (_, _) => await RewriteSelectionAsync(RewriteProvider.Ollama);
        }

        if (_hotkeyWindow.IsOpenAiRegistered)
        {
            _hotkeyWindow.OpenAiHotkeyPressed += async (_, _) => await RewriteSelectionAsync(RewriteProvider.OpenAi);
        }

        _logger.Info($"Application started. Settings file: {_settingsStore.SettingsPath}");
        ShowStartupStatus();
    }

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add($"Rewrite via Ollama ({_settings.HotkeyLabel})", null, async (_, _) => await RewriteSelectionAsync(RewriteProvider.Ollama));
        menu.Items.Add($"Rewrite via OpenAI ({_settings.OpenAiHotkeyLabel})", null, async (_, _) => await RewriteSelectionAsync(RewriteProvider.OpenAi));
        menu.Items.Add("Settings", null, (_, _) => OpenSettings());
        menu.Items.Add("Open log folder", null, (_, _) => OpenLogFolder());
        menu.Items.Add("Help", null, (_, _) => OpenHelp());
        menu.Items.Add("Exit", null, (_, _) => ExitThread());
        return menu;
    }

    private async Task RewriteSelectionAsync(RewriteProvider provider)
    {
        if (Interlocked.Exchange(ref _isBusy, 1) == 1)
        {
            CancellationTokenSource? activeRequest;
            lock (_requestSync)
            {
                _queuedProvider = provider;
                activeRequest = _activeRequestCts;
            }

            activeRequest?.Cancel();
            ShowInfo("Active request canceled. Retrying with the latest request ...");
            return;
        }

        string? selectedText = null;
        CancellationTokenSource? requestCts = null;
        try
        {
            requestCts = new CancellationTokenSource();
            lock (_requestSync)
            {
                _activeRequestCts = requestCts;
            }

            var cancellationToken = requestCts.Token;
            selectedText = await ClipboardWorkflow.CaptureSelectionAsync(_settings, cancellationToken);
            if (string.IsNullOrWhiteSpace(selectedText))
            {
                _logger.Info("No selected text found.");
                ShowInfo("No selected text found.");
                return;
            }

            _logger.Info($"Captured selection with {selectedText.Length} characters for {provider}.");
            ShowInfo($"Sending selection to {GetProviderDisplayName(provider)} ...");
            var rewritten = await GetClient(provider).RewriteAsync(selectedText, cancellationToken);
            _logger.LogRewrite(provider, selectedText, rewritten, succeeded: true);
            await ClipboardWorkflow.PasteReplacementAsync(rewritten, _settings, cancellationToken);
            _logger.Info($"Inserted response with {rewritten.Length} characters from {provider}.");
            ShowInfo("Response inserted.");
        }
        catch (OperationCanceledException)
        {
            _logger.Info($"{provider} rewrite request canceled.");
            ShowInfo("Request canceled.");
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrWhiteSpace(selectedText))
            {
                _logger.LogRewrite(provider, selectedText, responseText: null, succeeded: false, errorMessage: ex.ToString());
            }

            _logger.Error($"{provider} rewrite request failed.", ex);
            MessageBox.Show(ex.Message, "AIHotkey", MessageBoxButtons.OK, MessageBoxIcon.Error);
            ShowInfo("Request failed.");
        }
        finally
        {
            requestCts?.Dispose();
            lock (_requestSync)
            {
                if (ReferenceEquals(_activeRequestCts, requestCts))
                {
                    _activeRequestCts = null;
                }
            }

            Interlocked.Exchange(ref _isBusy, 0);

            RewriteProvider? queuedProvider;
            lock (_requestSync)
            {
                queuedProvider = _queuedProvider;
                _queuedProvider = null;
            }

            if (queuedProvider is { } nextProvider)
            {
                _ = RewriteSelectionAsync(nextProvider);
            }
        }
    }

    private IRewriteClient GetClient(RewriteProvider provider)
    {
        return provider switch
        {
            RewriteProvider.Ollama => _ollamaClient,
            RewriteProvider.OpenAi => _openAiClient,
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
        };
    }

    private string GetProviderDisplayName(RewriteProvider provider)
    {
        return provider switch
        {
            RewriteProvider.Ollama => "Ollama",
            RewriteProvider.OpenAi => "OpenAI",
            _ => provider.ToString()
        };
    }

    private void ShowStartupStatus()
    {
        var registered = new List<string>();
        if (_hotkeyWindow.IsOllamaRegistered)
        {
            registered.Add($"Ollama {_settings.HotkeyLabel}");
        }

        if (_hotkeyWindow.IsOpenAiRegistered)
        {
            registered.Add($"OpenAI {_settings.OpenAiHotkeyLabel}");
        }

        if (registered.Count > 0)
        {
            ShowInfo($"Ready. Hotkeys: {string.Join(", ", registered)}");
            return;
        }

        _logger.Error("Could not register any global hotkeys.");
        ShowInfo("Started without hotkeys. Use the tray menu.");
    }

    private void ShowInfo(string message)
    {
        _logger.Info(message);
        _notifyIcon.BalloonTipTitle = "AIHotkey";
        _notifyIcon.BalloonTipText = message;
        _notifyIcon.ShowBalloonTip(2000);
    }

    private void OpenSettings()
    {
        using var form = new SettingsForm(_settings);
        if (form.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        _settings = form.BuildSettings(_settings);
        _settingsStore.Save(_settings);
        _ollamaClient = OllamaClientFactory.Create(_settings);
        _openAiClient = OllamaClientFactory.CreateOpenAi(_settings);
        _notifyIcon.ContextMenuStrip = BuildMenu();
        _logger.Info("Settings updated.");
        ShowInfo("Settings saved.");
    }

    private void OpenLogFolder()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = _logger.LogDirectory,
            UseShellExecute = true
        });
    }

    private void OpenHelp()
    {
        using var form = new HelpForm(_settings, _settingsStore.SettingsPath, _logger.LogDirectory);
        form.ShowDialog();
    }

    protected override void ExitThreadCore()
    {
        _hotkeyWindow.Dispose();
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _trayIcon.Dispose();
        base.ExitThreadCore();
    }
}
