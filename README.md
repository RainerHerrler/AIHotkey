# AIHotkey

Dieses Repository enthält die Windows-Tray-Anwendung in C#.

Aktueller Stand:

- WinForms-Tray-App
- eigenes Systray-Icon mit `KI`
- globale Hotkeys `Ctrl+Shift+L` fuer Ollama und `Ctrl+Shift+O` fuer OpenAI
- kopiert markierten Text über die Zwischenablage
- sendet den Text je nach Hotkey an Ollama oder OpenAI
- fügt die Antwort per `Ctrl+V` wieder ein
- Tray-Menü mit strukturiertem Settings-Dialog, Verbindungstest und Hilfefenster
- persistente JSON-Settings unter `%LocalAppData%\\AIHotkey\\settings.json`
- Logdatei unter `%LocalAppData%\\AIHotkey\\logs\\aihotkey.log`
- JSONL-Tageslogs mit gesendetem und empfangenem Text unter `%LocalAppData%\\AIHotkey\\logs\\aihotkey-YYYY-MM-DD.jsonl`

## Dateien

- `AIHotkey.csproj`: Projektdatei für .NET 8 / Windows Forms
- `Program.cs`: Einstiegspunkt
- `TrayApplicationContext.cs`: Tray-Icon, Menü und Hotkey-Flow
- `HotkeyWindow.cs`: globale Hotkey-Registrierung
- `ClipboardWorkflow.cs`: Auswahl kopieren und Antwort einfügen
- `OllamaClient.cs`: HTTP-Client für Ollama
- `OpenAiClient.cs`: HTTP-Client für OpenAI Responses API
- `AppSettings.cs`: zentrale Defaults
- `SettingsStore.cs`: Laden und Speichern der JSON-Settings
- `SettingsForm.cs`: WinForms-Settings-Fenster für Ollama, OpenAI und gemeinsame Modellkonfiguration
- `AppLogger.cs`: Logdatei unter `%LocalAppData%`

## Build

Diese Variante musst du unter Windows mit installiertem .NET SDK bauen:

```powershell
dotnet build
dotnet run
```

## Settings

Beim ersten Start wird eine Settings-Datei unter `%LocalAppData%\\AIHotkey\\settings.json` angelegt.

Aktuelle Defaults:

- Ollama-URL: `http://ollama.local/api/generate`
- Modell: `gpt-oss:20b`
- Ollama-Hotkey: `Ctrl+Shift+L`
- OpenAI-Hotkey: `Ctrl+Shift+O`
- OpenAI-Responses-URL: `https://api.openai.com/v1/responses`
- OpenAI-Modell: `gpt-5-mini`
- Timeout: `120` Sekunden

Im Settings-Dialog kannst du die aktuell eingetragenen Werte jetzt direkt gegen Ollama und OpenAI testen.

## Hinweise

- Die erste Version arbeitet absichtlich einfach über Clipboard, `Ctrl+C` und `Ctrl+V`.
- Das funktioniert in vielen Programmen gut, aber nicht garantiert in jedem Spezialeditor.
- Der Hotkey ist aktuell fest auf `Ctrl+Shift+L` verdrahtet; die Anzeige dafür kommt aus den Settings, die Registrierung selbst noch nicht.
- Für eine robustere Version wären als Nächstes frei konfigurierbare Hotkeys, gezieltere Eingabesimulation und ein sauberer Editor/Prompt-Modus sinnvoll.
