using Avalonia.Controls.Templates;

namespace AvaloniaUI.Ribbon.Contracts;

public interface ICanAddToQuickAccess
{
    IControlTemplate QuickAccessTemplate { get; set; }

    bool CanAddToQuickAccess { get; set; }

    bool CanBeAddedToQat
    {
        get => CanAddToQuickAccess;
        set => CanAddToQuickAccess = value;
    }

    public object? QuickAccessIcon { get; set; }
}
