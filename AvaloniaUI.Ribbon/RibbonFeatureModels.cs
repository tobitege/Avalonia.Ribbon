using System;
using Avalonia.Controls;

namespace AvaloniaUI.Ribbon;

public enum RibbonItemPosition
{
    Before,
    After
}

public enum RibbonEventType
{
    Click
}

public enum RibbonVisualStyle
{
    Default,
    Light,
    Dark
}

public sealed class RibbonEventArgs : EventArgs
{
    public RibbonEventArgs(Control item, RibbonEventType eventType)
    {
        Item = item ?? throw new ArgumentNullException(nameof(item));
        EventType = eventType;
    }

    public Control Item { get; }

    public RibbonEventType EventType { get; }

    public bool Handled { get; set; }
}

public sealed record RibbonItemDefinition(string Name, string Text, bool QuickStart = false);
