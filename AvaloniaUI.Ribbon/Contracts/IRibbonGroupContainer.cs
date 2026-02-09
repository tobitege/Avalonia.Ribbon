using AvaloniaUI.Ribbon.Models;

namespace AvaloniaUI.Ribbon.Contracts;

public interface IRibbonGroupContainer
{
    GroupDisplayMode DisplayMode { get; set; }

    void ApplyDisplayMode(GroupDisplayMode displayMode);
}
