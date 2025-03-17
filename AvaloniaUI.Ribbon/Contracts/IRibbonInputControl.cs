using Avalonia.Controls.Templates;

namespace AvaloniaUI.Ribbon.Contracts;

public interface IRibbonInputControl : IRibbonControl
{
    public object Content { get; set; }

    public object Icon { get; set; }

    public object LargeIcon { get; set; }
}