using System.Windows.Input;
using Avalonia.Input;

namespace AvaloniaUI.Ribbon.Contracts;

public interface IRibbonCommand
{
    public ICommand? Command { get; set; }

    public object? CommandParameter { get; set; }

    public KeyGesture? ShortcutKeys { get; set; }
}