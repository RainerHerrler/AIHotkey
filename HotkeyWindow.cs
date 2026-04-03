using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AIHotkey;

internal sealed class HotkeyWindow : NativeWindow, IDisposable
{
    private const int WmHotkey = 0x0312;
    private const int ModControl = 0x0002;
    private const int ModShift = 0x0004;
    private const int VkL = 0x4C;
    private const int VkO = 0x4F;
    private const int OllamaHotkeyId = 1;
    private const int OpenAiHotkeyId = 2;

    private bool _ollamaRegistered;
    private bool _openAiRegistered;

    public event EventHandler? OllamaHotkeyPressed;
    public event EventHandler? OpenAiHotkeyPressed;

    public HotkeyWindow()
    {
        CreateHandle(new CreateParams());
        _ollamaRegistered = RegisterHotKey(Handle, OllamaHotkeyId, ModControl | ModShift, VkL);
        _openAiRegistered = RegisterHotKey(Handle, OpenAiHotkeyId, ModControl | ModShift, VkO);
    }

    public bool IsOllamaRegistered => _ollamaRegistered;

    public bool IsOpenAiRegistered => _openAiRegistered;

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WmHotkey)
        {
            switch (m.WParam.ToInt32())
            {
                case OllamaHotkeyId:
                    OllamaHotkeyPressed?.Invoke(this, EventArgs.Empty);
                    break;
                case OpenAiHotkeyId:
                    OpenAiHotkeyPressed?.Invoke(this, EventArgs.Empty);
                    break;
            }
        }

        base.WndProc(ref m);
    }

    public void Dispose()
    {
        if (_ollamaRegistered)
        {
            UnregisterHotKey(Handle, OllamaHotkeyId);
        }

        if (_openAiRegistered)
        {
            UnregisterHotKey(Handle, OpenAiHotkeyId);
        }

        DestroyHandle();
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
