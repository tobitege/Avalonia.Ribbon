using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
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

    public ObservableCollection<GalleryRange> Ranges
    {
        get => _ranges;
        set => SetAndRaise(RangesProperty, ref _ranges, value);
    }

    public event EventHandler<GalleryItemHoverChangedEventArgs>? ItemHoverChanged;

    public void BringIntoView(int index)
    {
        if (_scrollPresenter == null || _mainPresenter == null)
            return;

        if (index < 0 || index >= ItemCount || ItemHeight <= 0)
            return;

        var columns = GetColumnCount();
        var rowIndex = index / columns;
        var targetOffset = rowIndex * ItemHeight;
        var maxOffset = Math.Max(0, (_mainPresenter.Bounds.Height - _scrollPresenter.Bounds.Height));
        var clampedOffset = Math.Max(0, Math.Min(targetOffset, maxOffset));
        _scrollPresenter.Offset = _scrollPresenter.Offset.WithY(clampedOffset);
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
        _scrollPresenter = pres;
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

        RemoveHandler(InputElement.PointerEnteredEvent, OnItemPointerEntered);
        RemoveHandler(InputElement.PointerExitedEvent, OnItemPointerExited);
        AddHandler(InputElement.PointerEnteredEvent, OnItemPointerEntered, RoutingStrategies.Tunnel, true);
        AddHandler(InputElement.PointerExitedEvent, OnItemPointerExited, RoutingStrategies.Tunnel, true);

        UpdatePresenterLocation(IsDropDownOpen);
    }

    #region Static Properties

    public static readonly StyledProperty<bool> IsDropDownOpenProperty;

    public static readonly StyledProperty<double> ItemHeightProperty =
        AvaloniaProperty.Register<Gallery, double>(nameof(ItemHeight));

    public static readonly StyledProperty<RibbonControlSize> MaxSizeProperty;
    public static readonly StyledProperty<RibbonControlSize> MinSizeProperty;
    public static readonly StyledProperty<RibbonControlSize> SizeProperty;

    public static readonly DirectProperty<Gallery, ObservableCollection<GalleryRange>> RangesProperty =
        AvaloniaProperty.RegisterDirect<Gallery, ObservableCollection<GalleryRange>>(
            nameof(Ranges), o => o.Ranges, (o, v) => o.Ranges = v);

    #endregion Static Properties

    #region Fields

    private ContentControl? _flyoutPresenter;
    private ItemsPresenter? _itemsPresenter;
    private ContentControl? _mainPresenter;
    private ObservableCollection<GalleryRange> _ranges = new();
    private GalleryScrollContentPresenter? _scrollPresenter;

    #endregion Fields

    #region Methods

    private int GetColumnCount()
    {
        return Size switch
        {
            RibbonControlSize.Small => 1,
            RibbonControlSize.Medium => 2,
            _ => 3
        };
    }

    private void OnItemPointerEntered(object? sender, PointerEventArgs e)
    {
        if (!TryGetGalleryItem(e.Source, out var galleryItem))
            return;

        RaiseItemHoverChanged(galleryItem, true);
    }

    private void OnItemPointerExited(object? sender, PointerEventArgs e)
    {
        if (!TryGetGalleryItem(e.Source, out var galleryItem))
            return;

        RaiseItemHoverChanged(galleryItem, false);
    }

    private bool TryGetGalleryItem(object? source, out GalleryItem galleryItem)
    {
        if (source is GalleryItem item)
        {
            galleryItem = item;
            return true;
        }

        if (source is Visual visual)
        {
            var parentItem = visual.FindAncestorOfType<GalleryItem>();
            if (parentItem != null)
            {
                galleryItem = parentItem;
                return true;
            }
        }

        galleryItem = null!;
        return false;
    }

    private void RaiseItemHoverChanged(GalleryItem galleryItem, bool isHovering)
    {
        var indexedItems = Items.Cast<object>().ToList();
        var dataItem = galleryItem.DataContext ?? galleryItem;
        var index = indexedItems.FindIndex(item => ReferenceEquals(item, dataItem) || Equals(item, dataItem));
        var item = index >= 0 && index < indexedItems.Count ? indexedItems[index] : dataItem;
        ItemHoverChanged?.Invoke(this, new GalleryItemHoverChangedEventArgs(index, item, isHovering));
    }

    #endregion Methods
}