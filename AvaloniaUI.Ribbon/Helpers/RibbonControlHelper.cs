using System;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using AvaloniaUI.Ribbon.Contracts;
using AvaloniaUI.Ribbon.Models;

namespace AvaloniaUI.Ribbon.Helpers;

public static class RibbonControlHelper<T> where T : Layoutable
{
    private static readonly StyledProperty<RibbonControlSize> SizeProperty =
        AvaloniaProperty.Register<TemplatedControl, RibbonControlSize>("Size", RibbonControlSize.Large,
            coerce: CoerceSize);

    private static readonly StyledProperty<RibbonControlSize> MinSizeProperty =
        AvaloniaProperty.Register<TemplatedControl, RibbonControlSize>("MinSize");

    private static readonly StyledProperty<RibbonControlSize> MaxSizeProperty =
        AvaloniaProperty.Register<TemplatedControl, RibbonControlSize>("MaxSize", RibbonControlSize.Large);

    private static RibbonControlSize CoerceSize(AvaloniaObject obj, RibbonControlSize val)
    {
        if (obj is IRibbonControl ctrl)
        {
            if ((int)ctrl.MinSize > (int)val)
                return ctrl.MinSize;
            if ((int)ctrl.MaxSize < (int)val)
                return ctrl.MaxSize;
            return val;
        }

        throw new Exception("obj must be an IRibbonControl!");
    }

    public static void SetProperties(out StyledProperty<RibbonControlSize> size,
        out StyledProperty<RibbonControlSize> minSize, out StyledProperty<RibbonControlSize> maxSize)
    {
        size = SizeProperty;
        minSize = MinSizeProperty;
        maxSize = MaxSizeProperty;

        minSize.Changed.AddClassHandler<T>((sender, args) =>
        {
            if (sender is not IRibbonControl control || args.NewValue is not RibbonControlSize newValue)
                return;

            if ((int)newValue > (int)control.Size)
                control.Size = newValue;
        });

        maxSize.Changed.AddClassHandler<T>((sender, args) =>
        {
            if (sender is not IRibbonControl control || args.NewValue is not RibbonControlSize newValue)
                return;

            if ((int)newValue < (int)control.Size)
                control.Size = newValue;
        });
    }
}