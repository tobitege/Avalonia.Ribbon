using System;
using Avalonia;
using Avalonia.Controls;

namespace AvaloniaUI.Ribbon;

public sealed class RibbonItem : AvaloniaObject
{
    public static readonly AttachedProperty<string?> IdProperty =
        AvaloniaProperty.RegisterAttached<RibbonItem, AvaloniaObject, string?>("Id");

    private RibbonItem()
    {
    }

    public static string? GetId(AvaloniaObject element)
    {
        ArgumentNullException.ThrowIfNull(element);
        return element.GetValue(IdProperty);
    }

    public static void SetId(AvaloniaObject element, string? value)
    {
        ArgumentNullException.ThrowIfNull(element);
        element.SetValue(IdProperty, value);
    }

    internal static string? GetIdentity(Control control)
    {
        ArgumentNullException.ThrowIfNull(control);
        var id = GetId(control);
        return !string.IsNullOrWhiteSpace(id) ? id : control.Name;
    }

    internal static bool HasIdentity(Control control, string identity)
    {
        return !string.IsNullOrWhiteSpace(identity) &&
               string.Equals(GetIdentity(control), identity, StringComparison.Ordinal);
    }
}
