using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using AvaloniaUI.Ribbon.Contracts;
using AvaloniaUI.Ribbon.Models;
using VisualExtensions = Avalonia.VisualTree.VisualExtensions;

namespace AvaloniaUI.Ribbon.Desktop;

[TemplatePart("PART_CollapsedContentPopup", typeof(Popup))]
[TemplatePart("PART_SelectedGroupsHost", typeof(ItemsControl))]
[TemplatePart("PART_GroupsPresenterHolder", typeof(ContentControl))]
[TemplatePart("PART_PopupGroupsPresenterHolder", typeof(ContentControl))]
[TemplatePart("PART_ItemsPresenter", typeof(ItemsPresenter))]
[TemplatePart("PART_PinLastHoveredControlToQuickAccess", typeof(MenuItem))]
[TemplatePart("PART_ContentAreaContextMenu", typeof(ContextMenu))]
[TemplatePart("PART_CollapseRibbon", typeof(MenuItem))]
[TemplatePart("PART_QuickAccessToolbarHostAbove", typeof(ContentPresenter))]
[TemplatePart("PART_QuickAccessToolbarHostBelow", typeof(ContentPresenter))]
    public class DesktopRibbon : Ribbon
    {
        private bool _isSyncingQuickAccess;

        private ObservableCollection<ICanAddToQuickAccess>? _observedQuickAccessItems;

    private ContentPresenter? _quickAccessHostAbove;

    private ContentPresenter? _quickAccessHostBelow;

    #region Static Properties

    public static readonly StyledProperty<QuickAccessToolbar?> QuickAccessToolbarProperty =
        AvaloniaProperty.Register<DesktopRibbon, QuickAccessToolbar?>(nameof(QuickAccessToolbar));

    #endregion Static Properties

    static DesktopRibbon()
    {
        QuickAccessItemsProperty.Changed.AddClassHandler<DesktopRibbon>((sender, args) =>
            sender.OnQuickAccessItemsPropertyChanged(args));
        QuickAccessToolbarProperty.Changed.AddClassHandler<DesktopRibbon>((sender, args) =>
            sender.OnQuickAccessToolbarChanged(args));
        QuickAccessLocationProperty.Changed.AddClassHandler<DesktopRibbon>((sender, args) =>
            sender.ApplyQuickAccessLocation());
        ShowQatOverflowButtonProperty.Changed.AddClassHandler<DesktopRibbon>((sender, args) =>
            sender.ApplyQatOverflowButton());
    }

    public DesktopRibbon()
    {
        SetQuickAccessItemsCollection(QuickAccessItems);
    }

    #region Properties

    public QuickAccessToolbar? QuickAccessToolbar
    {
        get => GetValue(QuickAccessToolbarProperty);
        set => SetValue(QuickAccessToolbarProperty, value);
    }

    #endregion Properties

    protected override Type StyleKeyOverride => typeof(DesktopRibbon);

    #region Methods

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _quickAccessHostAbove = e.NameScope.Find<ContentPresenter>("PART_QuickAccessToolbarHostAbove");
        _quickAccessHostBelow = e.NameScope.Find<ContentPresenter>("PART_QuickAccessToolbarHostBelow");
        UpdatePresenterLocation(IsCollapsed);

        ApplyQatOverflowButton();
        ApplyQuickAccessLocation();
        SyncQuickAccessItemsFromRibbon();

        var pinToQat = e.NameScope.Find<MenuItem>("PART_PinLastHoveredControlToQuickAccess");
        if (pinToQat is not null)
            pinToQat.Click += (_, _) =>
            {
                if (_rightClicked != null)
                    ToggleQuickAccess(_rightClicked);
            };

        if (_groupsHost is not null)
        {
            _groupsHost.PointerExited += (_, _) =>
            {
                if (_ctxMenu == null || !_ctxMenu.IsOpen)
                    _rightClicked = null;
            };
            _groupsHost.AddHandler(PointerReleasedEvent,
                (_, args) =>
                {
                    if (args.Source is Visual visual && pinToQat is not null)
                    {
                        var ctrl = VisualExtensions.FindAncestorOfType<ICanAddToQuickAccess>(visual);

                        _rightClicked = ctrl;

                        if (QuickAccessToolbar != null)
                            pinToQat.IsEnabled = _rightClicked != null && _rightClicked.CanAddToQuickAccess;
                        else
                            pinToQat.IsEnabled = false;
                    }
                }, handledEventsToo: true);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        if (QuickAccessToolbar != null)
        {
            QuickAccessToolbar.ItemAdded -= QuickAccessToolbar_ItemChanged;
            QuickAccessToolbar.ItemRemoved -= QuickAccessToolbar_ItemChanged;
            QuickAccessToolbar.Ribbon = null;
        }

        if (_observedQuickAccessItems != null)
            _observedQuickAccessItems.CollectionChanged -= QuickAccessItemsCollectionChanged;

        if (_quickAccessHostAbove is not null)
            _quickAccessHostAbove.Content = null;

        if (_quickAccessHostBelow is not null)
            _quickAccessHostBelow.Content = null;

        _quickAccessHostAbove = null;
        _quickAccessHostBelow = null;
    }

    private void OnQuickAccessItemsPropertyChanged(AvaloniaPropertyChangedEventArgs args)
    {
        var oldItems = args.OldValue as ObservableCollection<ICanAddToQuickAccess>;
        var newItems = args.NewValue as ObservableCollection<ICanAddToQuickAccess>;

        if (oldItems != null)
            oldItems.CollectionChanged -= QuickAccessItemsCollectionChanged;

        SetQuickAccessItemsCollection(newItems);
        SyncQuickAccessItemsFromRibbon();
    }

    private void OnQuickAccessToolbarChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (args.OldValue is QuickAccessToolbar oldToolbar)
        {
            oldToolbar.ItemAdded -= QuickAccessToolbar_ItemChanged;
            oldToolbar.ItemRemoved -= QuickAccessToolbar_ItemChanged;
            oldToolbar.Ribbon = null;
        }

        if (_quickAccessHostAbove is not null)
            _quickAccessHostAbove.Content = null;

        if (_quickAccessHostBelow is not null)
            _quickAccessHostBelow.Content = null;

        if (args.NewValue is QuickAccessToolbar newToolbar)
        {
            newToolbar.ItemAdded += QuickAccessToolbar_ItemChanged;
            newToolbar.ItemRemoved += QuickAccessToolbar_ItemChanged;
            newToolbar.Ribbon = this;
            newToolbar.ShowOverflowButton = ShowQatOverflowButton;
        }

        ApplyQuickAccessLocation();
        SyncQuickAccessItemsFromRibbon();
    }

    private void SetQuickAccessItemsCollection(ObservableCollection<ICanAddToQuickAccess>? items)
    {
        if (_observedQuickAccessItems != null)
            _observedQuickAccessItems.CollectionChanged -= QuickAccessItemsCollectionChanged;

        _observedQuickAccessItems = items;

        if (_observedQuickAccessItems != null)
            _observedQuickAccessItems.CollectionChanged += QuickAccessItemsCollectionChanged;
    }

    private void QuickAccessItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SyncQuickAccessItemsFromRibbon();
    }

    private void ApplyQatOverflowButton()
    {
        if (QuickAccessToolbar == null)
            return;

        QuickAccessToolbar.ShowOverflowButton = ShowQatOverflowButton;
    }

    private void ApplyQuickAccessLocation()
    {
        if (QuickAccessToolbar == null)
            return;

        if (QuickAccessLocation == RibbonQatLocation.Hidden)
        {
            QuickAccessToolbar.IsVisible = false;
            return;
        }

        if (_quickAccessHostAbove == null || _quickAccessHostBelow == null)
        {
            QuickAccessToolbar.IsVisible = true;
            return;
        }

        _quickAccessHostAbove.Content = null;
        _quickAccessHostBelow.Content = null;

        if (VisualExtensions.FindAncestorOfType<RibbonWindow>(this) is null)
        {
            QuickAccessToolbar.IsVisible = true;

            if (QuickAccessLocation == RibbonQatLocation.Above)
                _quickAccessHostAbove.Content = QuickAccessToolbar;
            else if (QuickAccessLocation == RibbonQatLocation.Below)
                _quickAccessHostBelow.Content = QuickAccessToolbar;
        }
        else
        {
            QuickAccessToolbar.IsVisible = true;
        }
    }

    private void SyncQuickAccessItemsFromRibbon()
    {
        if (QuickAccessToolbar == null || _isSyncingQuickAccess)
            return;

        _isSyncingQuickAccess = true;
        try
        {
            QuickAccessToolbar.SyncItems(QuickAccessItems);
        }
        finally
        {
            _isSyncingQuickAccess = false;
        }
    }

    private void QuickAccessToolbar_ItemChanged(object? sender, ICanAddToQuickAccess? item)
    {
        if (_isSyncingQuickAccess || item is null || QuickAccessToolbar is null)
            return;

        if (QuickAccessItems.Contains(item))
        {
            _isSyncingQuickAccess = true;
            QuickAccessItems.Remove(item);
            _isSyncingQuickAccess = false;
        }
        else
        {
            _isSyncingQuickAccess = true;
            QuickAccessItems.Add(item);
            _isSyncingQuickAccess = false;
        }
    }

    private void ToggleQuickAccess(ICanAddToQuickAccess? item)
    {
        if (item is null || QuickAccessToolbar == null)
            return;

        if (QuickAccessToolbar.ContainsItem(item))
            QuickAccessToolbar.RemoveItem(item);
        else
            QuickAccessToolbar.AddItem(item);
    }

    #endregion Methods
}
