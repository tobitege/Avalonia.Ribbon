using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;

namespace AvaloniaUI.Ribbon;

public static class RibbonBuilder
{
    public static RibbonTab InsertOrAddTab(
        Ribbon ribbon,
        string name,
        string text,
        string? afterName = null)
    {
        ArgumentNullException.ThrowIfNull(ribbon);
        ValidateName(name);

        var existing = FindNamed<RibbonTab>(ribbon.Tabs, name);
        if (existing is not null)
        {
            existing.Header = text;
            return existing;
        }

        var tab = new RibbonTab { Name = name, Header = text };
        InsertAfterOrAppend(ribbon.Tabs, tab, afterName);
        return tab;
    }

    public static RibbonGroupBox InsertOrAddGroup(
        RibbonTab tab,
        string name,
        string text,
        string? afterName = null)
    {
        ArgumentNullException.ThrowIfNull(tab);
        ValidateName(name);

        var existing = FindNamed<RibbonGroupBox>(tab.Groups, name);
        if (existing is not null)
        {
            existing.Header = text;
            return existing;
        }

        var group = new RibbonGroupBox { Name = name, Header = text };
        InsertAfterOrAppend(tab.Groups, group, afterName);
        return group;
    }

    public static RibbonButton InsertOrAddButton(
        RibbonGroupBox group,
        string name,
        string text,
        object? largeImage,
        object? smallImage,
        object? tooltip,
        string? afterName = null,
        bool canBeAddedToQat = true)
    {
        ArgumentNullException.ThrowIfNull(group);
        ValidateName(name);

        var existing = FindNamed<RibbonButton>(group.Items.OfType<Control>(), name);
        if (existing is null)
        {
            existing = new RibbonButton { Name = name };
            InsertAfterOrAppend(group.Items, existing, afterName);
        }

        existing.Content = text;
        existing.LargeIcon = largeImage;
        existing.Icon = smallImage;
        existing.QuickAccessIcon = smallImage;
        existing.CanAddToQuickAccess = canBeAddedToQat;
        ToolTip.SetTip(existing, tooltip);
        return existing;
    }

    public static RibbonContextualTabGroup InsertOrAddContextualTabGroup(
        Ribbon ribbon,
        string name,
        string text,
        string? afterName = null)
    {
        ArgumentNullException.ThrowIfNull(ribbon);
        ValidateName(name);

        var existing = FindNamed<RibbonContextualTabGroup>(ribbon.Tabs, name);
        if (existing is not null)
        {
            existing.Header = text;
            return existing;
        }

        var group = new RibbonContextualTabGroup { Name = name, Header = text };
        InsertAfterOrAppend(ribbon.Tabs, group, afterName);
        return group;
    }

    public static RibbonTab InsertOrAddTabToContextualTabGroup(
        RibbonContextualTabGroup tabGroup,
        string name,
        string text,
        string? afterName = null)
    {
        ArgumentNullException.ThrowIfNull(tabGroup);
        ValidateName(name);

        var existing = FindNamed<RibbonTab>(tabGroup.Items.OfType<Control>(), name);
        if (existing is not null)
        {
            existing.Header = text;
            return existing;
        }

        var tab = new RibbonTab { Name = name, Header = text, IsContextual = true };
        InsertAfterOrAppend(tabGroup.Items, tab, afterName);
        return tab;
    }

    private static T? FindNamed<T>(IEnumerable<Control> controls, string name) where T : Control
    {
        return controls.OfType<T>()
            .FirstOrDefault(control => string.Equals(control.Name, name, StringComparison.Ordinal));
    }

    private static void InsertAfterOrAppend(IList items, Control item, string? afterName)
    {
        if (!string.IsNullOrWhiteSpace(afterName))
        {
            for (var index = 0; index < items.Count; index++)
            {
                if (items[index] is not Control anchor ||
                    !string.Equals(anchor.Name, afterName, StringComparison.Ordinal))
                    continue;

                if (index < items.Count - 1)
                {
                    items.Insert(index + 1, item);
                    return;
                }

                break;
            }
        }

        items.Add(item);
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Ribbon item names must be non-empty and stable.", nameof(name));
    }
}
