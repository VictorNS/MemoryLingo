using SW = System.Windows;

namespace MemoryLingo.Core.Services;

public interface ITrayService
{
    void Initialize(SW.Window window);
    void ShowWindow();
    void HideWindow();
    void Dispose();
}

public class TrayService : ITrayService, IDisposable
{
    private NotifyIcon? _notifyIcon;
    private SW.Window? _window;

    public void Initialize(SW.Window window)
    {
        _window = window;

        _notifyIcon = new NotifyIcon
        {
			Icon = new Icon(SW.Application.GetResourceStream(new Uri("pack://application:,,,/Presentation/Assets/Images/dyslexia-32.ico")).Stream),
            Text = "Memory Lingo",
            Visible = true
        };

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Show Memory Lingo", null, (s, e) => ShowWindow());
        contextMenu.Items.Add("-");
        contextMenu.Items.Add("Exit", null, (s, e) => SW.Application.Current.Shutdown());

        _notifyIcon.ContextMenuStrip = contextMenu;
        _notifyIcon.DoubleClick += (s, e) => ShowWindow();
    }

    public void ShowWindow()
    {
        if (_window != null)
        {
            _window.Show();
            _window.WindowState = SW.WindowState.Normal;
            _window.Activate();
        }
    }

    public void HideWindow()
    {
        _window?.Hide();
    }

    public void Dispose()
    {
        _notifyIcon?.Dispose();
    }
}
