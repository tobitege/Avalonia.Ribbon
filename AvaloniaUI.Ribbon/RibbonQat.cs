using System;
using System.Collections.ObjectModel;
using AvaloniaUI.Ribbon.Contracts;
using AvaloniaUI.Ribbon.Models;

namespace AvaloniaUI.Ribbon;

public sealed class RibbonQat
{
    private readonly Ribbon _ribbon;

    internal RibbonQat(Ribbon ribbon)
    {
        _ribbon = ribbon ?? throw new ArgumentNullException(nameof(ribbon));
    }

    public ObservableCollection<ICanAddToQuickAccess> Items => _ribbon.QuickAccessItems;

    public bool BelowRibbon
    {
        get => _ribbon.QuickAccessLocation == RibbonQatLocation.Below;
        set => _ribbon.SetCurrentValue(
            Ribbon.QuickAccessLocationProperty,
            value ? RibbonQatLocation.Below : RibbonQatLocation.Above);
    }
}
