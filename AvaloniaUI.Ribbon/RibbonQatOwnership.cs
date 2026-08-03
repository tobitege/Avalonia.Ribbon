using System;
using System.Collections.Generic;
using System.Linq;
using AvaloniaUI.Ribbon.Contracts;

namespace AvaloniaUI.Ribbon;

public sealed class RibbonQatOwnership
{
    private readonly Func<RibbonTab?> _viewTabProvider;

    public RibbonQatOwnership(Func<RibbonTab?> viewTabProvider)
    {
        _viewTabProvider = viewTabProvider ?? throw new ArgumentNullException(nameof(viewTabProvider));
    }

    public bool IsViewOwned(ICanAddToQuickAccess item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return EnumerateViewItems().Any(candidate => ReferenceEquals(candidate, item));
    }

    public IEnumerable<ICanAddToQuickAccess> EnumerateViewItems()
    {
        var tab = _viewTabProvider();
        if (tab is null)
            yield break;

        foreach (var item in RibbonItemLookup.EnumerateOwnedControls(tab)
                     .Skip(1)
                     .OfType<ICanAddToQuickAccess>())
        {
            yield return item;
        }
    }
}
