using System;

namespace AvaloniaUI.Ribbon.Models;

public sealed class GalleryItemHoverChangedEventArgs : EventArgs
{
    public GalleryItemHoverChangedEventArgs(int index, object? item, bool isHovering)
    {
        Index = index;
        Item = item;
        IsHovering = isHovering;
    }

    public int Index { get; }

    public object? Item { get; }

    public bool IsHovering { get; }
}
