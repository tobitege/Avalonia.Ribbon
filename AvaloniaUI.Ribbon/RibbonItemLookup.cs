using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace AvaloniaUI.Ribbon;

public sealed class RibbonItemLookup
{
    private readonly Ribbon _ribbon;

    public RibbonItemLookup(Ribbon ribbon)
    {
        _ribbon = ribbon ?? throw new ArgumentNullException(nameof(ribbon));
    }

    public RibbonTab? FindTab(string name)
    {
        return GetItem<RibbonTab>(name);
    }

    public static RibbonGroupBox? FindGroup(RibbonTab tab, string name)
    {
        ArgumentNullException.ThrowIfNull(tab);
        return tab.Groups.FirstOrDefault(group => HasIdentity(group, name));
    }

    public static T? FindItem<T>(RibbonGroupBox group, string name) where T : Control
    {
        ArgumentNullException.ThrowIfNull(group);

        return EnumerateOwnedControls(group)
            .Skip(1)
            .OfType<T>()
            .FirstOrDefault(item => HasIdentity(item, name));
    }

    public T? GetItem<T>(string name) where T : Control
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return EnumerateItems().OfType<T>().FirstOrDefault(item => HasIdentity(item, name));
    }

    public bool SetItemState(string name, bool? visible, bool? enabled)
    {
        var item = GetItem<Control>(name);
        if (item is null)
            return false;

        if (visible.HasValue)
            item.SetCurrentValue(Visual.IsVisibleProperty, visible.Value);
        if (enabled.HasValue)
            item.SetCurrentValue(InputElement.IsEnabledProperty, enabled.Value);

        return true;
    }

    public bool SetItemPosition(string name, string anchorName, RibbonItemPosition position)
    {
        var item = GetItem<Control>(name);
        var anchor = GetItem<Control>(anchorName);
        if (item is null || anchor is null)
            return false;
        if (ReferenceEquals(item, anchor))
            return true;

        foreach (var list in EnumeratePositionableLists())
        {
            var itemIndex = IndexOfReference(list, item);
            if (itemIndex < 0 || IndexOfReference(list, anchor) < 0)
                continue;

            list.RemoveAt(itemIndex);
            var anchorIndex = IndexOfReference(list, anchor);
            var targetIndex = position == RibbonItemPosition.Before ? anchorIndex : anchorIndex + 1;
            list.Insert(targetIndex, item);
            return true;
        }

        return false;
    }

    public IEnumerable<Control> EnumerateItems()
    {
        var seen = new HashSet<Control>(ReferenceEqualityComparer.Instance);

        foreach (var root in EnumerateRoots())
        {
            foreach (var item in EnumerateOwnedControls(root))
            {
                if (seen.Add(item))
                    yield return item;
            }
        }
    }

    internal static IEnumerable<Control> EnumerateOwnedControls(Control root)
    {
        var pending = new Stack<Control>();
        var seen = new HashSet<Control>(ReferenceEqualityComparer.Instance);
        pending.Push(root);

        while (pending.Count > 0)
        {
            var current = pending.Pop();
            if (!seen.Add(current))
                continue;

            yield return current;

            var children = GetOwnedChildren(current).ToArray();
            for (var index = children.Length - 1; index >= 0; index--)
                pending.Push(children[index]);
        }
    }

    private IEnumerable<Control> EnumerateRoots()
    {
        foreach (var tab in _ribbon.Tabs)
            yield return tab;

        if (_ribbon.ApplicationMenu is Control applicationMenu)
            yield return applicationMenu;

        yield return _ribbon.ConfigToolBar;
    }

    private static IEnumerable<Control> GetOwnedChildren(Control control)
    {
        switch (control)
        {
            case RibbonContextualTabGroup contextualGroup:
                return contextualGroup.Items.OfType<Control>();
            case RibbonTab tab:
                return tab.Groups;
            case RibbonGroupBox group:
                return group.Items.OfType<Control>();
            case RibbonGroupContainer container:
                return container.Children.OfType<Control>();
            case ItemsControl itemsControl:
                return itemsControl.Items.OfType<Control>();
            case Panel panel:
                return panel.Children;
            case ContentControl contentControl when contentControl.Content is Control child:
                return new[] { child };
            default:
                return Array.Empty<Control>();
        }
    }

    private IEnumerable<IList> EnumeratePositionableLists()
    {
        yield return _ribbon.Tabs;

        foreach (var root in _ribbon.Tabs)
        {
            foreach (var list in EnumeratePositionableLists(root))
                yield return list;
        }

        if (_ribbon.ApplicationMenu is Control applicationMenu)
        {
            foreach (var list in EnumeratePositionableLists(applicationMenu))
                yield return list;
        }

        foreach (var list in EnumeratePositionableLists(_ribbon.ConfigToolBar))
            yield return list;
    }

    private static IEnumerable<IList> EnumeratePositionableLists(Control owner)
    {
        IList? children = owner switch
        {
            RibbonContextualTabGroup contextualGroup => contextualGroup.Items,
            RibbonTab tab => tab.Groups,
            RibbonGroupBox group => group.Items,
            RibbonGroupContainer container => container.Children,
            ItemsControl itemsControl => itemsControl.Items,
            Panel panel => panel.Children,
            _ => null
        };

        if (children is null)
            yield break;

        yield return children;

        foreach (var child in children.OfType<Control>())
        {
            foreach (var nested in EnumeratePositionableLists(child))
                yield return nested;
        }
    }

    private static int IndexOfReference(IList list, object target)
    {
        for (var index = 0; index < list.Count; index++)
        {
            if (ReferenceEquals(list[index], target))
                return index;
        }

        return -1;
    }

    private static bool HasIdentity(Control control, string identity)
    {
        return RibbonItem.HasIdentity(control, identity);
    }
}

public static class RibbonCollectionExtensions
{
    public static bool Contains(this IEnumerable<Control> controls, string name)
    {
        ArgumentNullException.ThrowIfNull(controls);
        return !string.IsNullOrWhiteSpace(name) &&
               controls.Any(control => RibbonItem.HasIdentity(control, name));
    }
}
