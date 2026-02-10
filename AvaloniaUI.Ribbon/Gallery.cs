using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using AvaloniaUI.Ribbon.Contracts;
using AvaloniaUI.Ribbon.Helpers;
using AvaloniaUI.Ribbon.Models;

namespace AvaloniaUI.Ribbon;

[TemplatePart("PART_ItemsPresenter", typeof(ItemsPresenter))]
[TemplatePart("PART_ItemsPresenterHolder", typeof(ContentControl))]
[TemplatePart("PART_UpButton", typeof(RepeatButton))]
[TemplatePart("PART_DownButton", typeof(RepeatButton))]
[TemplatePart("PART_ScrollContentPresenter", typeof(GalleryScrollContentPresenter))]
[TemplatePart("PART_FlyoutItemsPresenterHolder", typeof(ContentControl))]
[TemplatePart("PART_FlyoutRoot", typeof(Control))]
public class Gallery : ListBox, IRibbonControl
{
    static Gallery()
    {
        //IsDropDownOpenProperty = ComboBox.IsDropDownOpenProperty.AddOwner<Gallery>(element => element.IsDropDownOpen, (element, value) => element.IsDropDownOpen = value);
        IsDropDownOpenProperty = ComboBox.IsDropDownOpenProperty.AddOwner<Gallery>();
        IsDropDownOpenProperty.Changed.AddClassHandler<Gallery, bool>((sneder, args) =>
        {
            if (args.NewValue.Value is bool value)
                sneder.UpdatePresenterLocation(value);
        });
        RibbonControlHelper<Gallery>.SetProperties(out SizeProperty, out MinSizeProperty, out MaxSizeProperty);
    }

    protected override Type StyleKeyOverride => typeof(Gallery);

    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    public double ItemHeight
    {
        get => GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    public RibbonControlSize MaxSize
    {
        get => GetValue(MaxSizeProperty);
        set => SetValue(MaxSizeProperty, value);
    }

    public RibbonControlSize MinSize
    {
        get => GetValue(MinSizeProperty);
        set => SetValue(MinSizeProperty, value);
    }

    public RibbonControlSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    private void UpdatePresenterLocation(bool intoFlyout)
    {
        if (_itemsPresenter == null || _mainPresenter == null || _flyoutPresenter == null)
            return;

        if (_itemsPresenter.Parent is ContentPresenter presenter)
            presenter.Content = null;
        else if (_itemsPresenter.Parent is ContentControl control)
            control.Content = null;
        else if (_itemsPresenter.Parent is Panel panel)
            panel.Children.Remove(_itemsPresenter);

        if (intoFlyout)
            _flyoutPresenter.Content = _itemsPresenter;
        else
            _mainPresenter.Content = _itemsPresenter;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _itemsPresenter = e.NameScope.Find<ItemsPresenter>("PART_ItemsPresenter");
        _mainPresenter = e.NameScope.Find<ContentControl>("PART_ItemsPresenterHolder");

        var pres = e.NameScope.Find<GalleryScrollContentPresenter>("PART_ScrollContentPresenter");
        var upButton = e.NameScope.Find<RepeatButton>("PART_UpButton");
        var downButton = e.NameScope.Find<RepeatButton>("PART_DownButton");
        if (pres != null && upButton != null)
        {
            upButton.Click += (_, _) =>
                pres.Offset = pres.Offset.WithY(Math.Max(0, pres.Offset.Y - ItemHeight));
        }

        if (pres != null && downButton != null)
        {
            downButton.Click += (_, _) =>
                pres.Offset = pres.Offset.WithY(Math.Min(pres.Offset.Y + ItemHeight,
                    (_mainPresenter?.Bounds.Height ?? 0) - pres.Bounds.Height));
        }

        _flyoutPresenter = e.NameScope.Find<ContentControl>("PART_FlyoutItemsPresenterHolder");
        /*_flyoutPresenter.PointerWheelChanged += (s, a) =>
        {
            a.Handled = true;
        };*/
        var flyoutRoot = e.NameScope.Find<Control>("PART_FlyoutRoot");
        if (flyoutRoot != null)
            flyoutRoot.PointerExited += (_, _) => IsDropDownOpen = false;

        UpdatePresenterLocation(IsDropDownOpen);
    }

    #region Static Properties

    public static readonly StyledProperty<bool> IsDropDownOpenProperty;

    public static readonly StyledProperty<double> ItemHeightProperty =
        AvaloniaProperty.Register<Gallery, double>(nameof(ItemHeight));

    public static readonly StyledProperty<RibbonControlSize> MaxSizeProperty;
    public static readonly StyledProperty<RibbonControlSize> MinSizeProperty;
    public static readonly StyledProperty<RibbonControlSize> SizeProperty;

    #endregion Static Properties

    #region Fields

    private ContentControl? _flyoutPresenter;
    private ItemsPresenter? _itemsPresenter;
    private ContentControl? _mainPresenter;

    #endregion Fields
}