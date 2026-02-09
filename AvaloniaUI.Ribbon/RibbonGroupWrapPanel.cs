using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using AvaloniaUI.Ribbon.Contracts;
using AvaloniaUI.Ribbon.Models;

namespace AvaloniaUI.Ribbon;

public class RibbonGroupWrapPanel : WrapPanel
{
    public static readonly StyledProperty<GroupDisplayMode> DisplayModeProperty =
        RibbonGroupBox.DisplayModeProperty
            .AddOwner<
                RibbonGroupWrapPanel>(); //AvaloniaProperty.Register<RibbonGroupWrapPanel, GroupDisplayMode>(nameof(DisplayMode), defaultValue: GroupDisplayMode.Large);

    static RibbonGroupWrapPanel()
    {
        AffectsArrange<RibbonGroupWrapPanel>(DisplayModeProperty);
        AffectsMeasure<RibbonGroupWrapPanel>(DisplayModeProperty);
        AffectsRender<RibbonGroupWrapPanel>(DisplayModeProperty);

        DisplayModeProperty.Changed.AddClassHandler<RibbonGroupWrapPanel>((sender, args) =>
        {
            sender.ApplyDisplayMode((GroupDisplayMode)args.NewValue);
        });
    }

    public RibbonGroupWrapPanel()
    {
        Children.CollectionChanged += (_, _) => ApplyDisplayMode(DisplayMode);

        if (TemplatedParent is RibbonGroupBox parentBox)
        {
            parentBox.Rearranged += (_, _) => ArrangeOverride(Bounds.Size);
            parentBox.Remeasured += (_, _) => MeasureOverride(Bounds.Size);
        }
    }

    public GroupDisplayMode DisplayMode
    {
        get => GetValue(DisplayModeProperty);
        set => SetValue(DisplayModeProperty, value);
    }

    private void ApplyDisplayMode(GroupDisplayMode displayMode)
    {
        Orientation = displayMode == GroupDisplayMode.Small ? Orientation.Vertical : Orientation.Horizontal;

        for (var i = 0; i < Children.Count; i++)
        {
            if (Children[i] is IRibbonGroupContainer groupContainer)
                groupContainer.ApplyDisplayMode(displayMode);

            if (Children[i] is IRibbonControl control)
                control.Size = displayMode == GroupDisplayMode.Small ? control.MinSize : control.MaxSize;
        }
    }
}
