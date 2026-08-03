using System;
using System.Collections.Generic;

namespace AvaloniaUI.Ribbon;

public sealed class RibbonEventRouter : IDisposable
{
    private readonly Dictionary<string, Action<RibbonEventArgs>> _handlers =
        new(StringComparer.Ordinal);
    private readonly Ribbon _ribbon;
    private bool _disposed;

    public RibbonEventRouter(Ribbon ribbon)
    {
        _ribbon = ribbon ?? throw new ArgumentNullException(nameof(ribbon));
        _ribbon.RibbonEvent += OnRibbonEvent;
    }

    public void Register(string itemName, Action<RibbonEventArgs> handler)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (string.IsNullOrWhiteSpace(itemName))
            throw new ArgumentException("A stable item name is required.", nameof(itemName));

        _handlers[itemName] = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    public bool Unregister(string itemName)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _handlers.Remove(itemName);
    }

    public void Route(RibbonEventArgs args)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(args);

        var name = args.Item.Name;
        if (string.IsNullOrWhiteSpace(name) || _ribbon.GetItemByName(name) is not { } currentItem)
            return;

        var currentArgs = ReferenceEquals(args.Item, currentItem)
            ? args
            : new RibbonEventArgs(currentItem, args.EventType) { Handled = args.Handled };

        if (currentArgs.EventType == RibbonEventType.Click && TryOpenSplitButton(currentItem))
        {
            currentArgs.Handled = true;
            args.Handled = true;
            return;
        }

        if (_handlers.TryGetValue(name, out var handler))
            handler(currentArgs);

        args.Handled = currentArgs.Handled;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _ribbon.RibbonEvent -= OnRibbonEvent;
        _handlers.Clear();
        _disposed = true;
    }

    private void OnRibbonEvent(object? sender, RibbonEventArgs args)
    {
        Route(args);
    }

    private bool TryOpenSplitButton(Avalonia.Controls.Control item)
    {
        if (_ribbon.Minimized)
            return item is RibbonSplitButton or SplitButtonControl;

        switch (item)
        {
            case RibbonSplitButton splitButton:
                splitButton.IsDropDownOpen = true;
                return true;
            case SplitButtonControl splitButton:
                splitButton.IsDropDownOpen = true;
                return true;
            default:
                return false;
        }
    }
}
