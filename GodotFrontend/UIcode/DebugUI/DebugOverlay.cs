using Godot;
using Godot.NativeInterop;
using System;

public partial class DebugOverlay : CanvasLayer
{
	private RichTextLabel _logText;

	
	private Label playerInfo;

	public static DebugOverlay Instance { get; private set; }

	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _Ready()
	{
		// Usamos GetNodeOrNull para evitar crashes si el nodo no existe
		_logText = GetNodeOrNull<RichTextLabel>("%LogText");
		playerInfo = GetNodeOrNull<Label>("%CurrentPlayer");

		if (_logText != null)
		{
			_logText.MouseFilter = Control.MouseFilterEnum.Ignore;
			_logText.BbcodeEnabled = true;
		}
	}
	public void changeCurrentPlayerLabel(string info)
	{
		CallDeferred("safeChangeCurrentPlayerLabel", info);
	}
    public void safeChangeCurrentPlayerLabel(string info)
    {
		playerInfo.Text = info;
    }
    public void Log(string message, Color? color = null)
	{
        if (_logText == null) return;

        // We wrap the UI logic in a separate method or a lambda
        // to call it via CallDeferred
		
        CallDeferred("SafeLog", message, color ?? Colors.White);
    }
    // This method will ALWAYS run on the Main Thread
    private void SafeLog(string message, Color textColor)
    {
        string hex = textColor.ToHtml(false);
        string timestamp = DateTime.Now.ToString("HH:mm:ss");

        string formatted = $"[color=#888888][{timestamp}][/color] [color=#{hex}]{message}[/color]\n";

        _logText.AppendText(formatted);
        _logText.ScrollToLine(_logText.GetLineCount());
    }
}
