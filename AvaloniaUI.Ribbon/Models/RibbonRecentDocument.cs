using System.Windows.Input;

namespace AvaloniaUI.Ribbon.Models;

public class RibbonRecentDocument
{
    public string? Title { get; set; }

    public string? Path { get; set; }

    public object? Icon { get; set; }

    public ICommand? Command { get; set; }

    public object? CommandParameter { get; set; }
}
