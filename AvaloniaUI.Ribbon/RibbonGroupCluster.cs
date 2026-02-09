using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using AvaloniaUI.Ribbon.Models;

namespace AvaloniaUI.Ribbon;

public class RibbonGroupCluster : RibbonGroupContainer
{
    private static readonly string[] ClusterClasses = { "cluster-first", "cluster-middle", "cluster-last", "cluster-single" };

    public RibbonGroupCluster()
    {
        MaximumSize = RibbonControlSize.Medium;
        MinimumSize = RibbonControlSize.Small;
        ItemSpacing = 0;
    }

    protected override RibbonControlSize ResolveTargetSize(GroupDisplayMode displayMode)
    {
        var requestedSize = base.ResolveTargetSize(displayMode);
        return requestedSize == RibbonControlSize.Large ? RibbonControlSize.Medium : requestedSize;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        EnsureChildControlSizes();

        var visibleChildren = GetVisibleChildren();
        UpdateClusterClasses(visibleChildren);

        var desiredWidth = 0d;
        var desiredHeight = 0d;
        var spacing = Math.Max(0, ItemSpacing);

        for (var i = 0; i < visibleChildren.Count; i++)
        {
            var child = visibleChildren[i];
            child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var desired = child.DesiredSize;

            desiredWidth += desired.Width;
            if (i > 0)
                desiredWidth += spacing;

            if (desiredHeight < desired.Height)
                desiredHeight = desired.Height;
        }

        return new Size(desiredWidth, desiredHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var visibleChildren = GetVisibleChildren();
        UpdateClusterClasses(visibleChildren);

        var x = 0d;
        var spacing = Math.Max(0, ItemSpacing);
        var desiredWidth = 0d;
        var desiredHeight = 0d;

        for (var i = 0; i < visibleChildren.Count; i++)
        {
            var child = visibleChildren[i];
            var desired = child.DesiredSize;

            if (i > 0)
                x += spacing;

            child.Arrange(new Rect(x, 0, desired.Width, desired.Height));
            x += desired.Width;

            desiredWidth = x;
            if (desiredHeight < desired.Height)
                desiredHeight = desired.Height;
        }

        return new Size(desiredWidth, desiredHeight);
    }

    private IReadOnlyList<Control> GetVisibleChildren()
    {
        var visibleChildren = new List<Control>();

        for (var i = 0; i < Children.Count; i++)
        {
            if (Children[i].IsVisible)
                visibleChildren.Add(Children[i]);
        }

        return visibleChildren;
    }

    private void UpdateClusterClasses(IReadOnlyList<Control> visibleChildren)
    {
        for (var i = 0; i < Children.Count; i++)
        {
            var child = Children[i];
            for (var classIndex = 0; classIndex < ClusterClasses.Length; classIndex++)
                child.Classes.Remove(ClusterClasses[classIndex]);
        }

        if (visibleChildren.Count == 0)
            return;

        if (visibleChildren.Count == 1)
        {
            visibleChildren[0].Classes.Add(ClusterClasses[3]);
            return;
        }

        for (var i = 0; i < visibleChildren.Count; i++)
        {
            if (i == 0)
            {
                visibleChildren[i].Classes.Add(ClusterClasses[0]);
                continue;
            }

            if (i == visibleChildren.Count - 1)
            {
                visibleChildren[i].Classes.Add(ClusterClasses[2]);
                continue;
            }

            visibleChildren[i].Classes.Add(ClusterClasses[1]);
        }
    }
}
