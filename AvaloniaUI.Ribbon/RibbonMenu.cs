using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using AvaloniaUI.Ribbon.Contracts;
using AvaloniaUI.Ribbon.Models;

namespace AvaloniaUI.Ribbon;

// TemplatePart attribute specifying the part "MenuPopup" required for this control
[TemplatePart("MenuPopup", typeof(Popup), IsRequired = true)]
public sealed class RibbonMenu : ItemsControl, IRibbonMenu
{
    // AvaloniaProperty for TopDockedGroupedItems, a collection of RibbonMenuItem groups
    public static readonly DirectProperty<RibbonMenu, IEnumerable<IGrouping<string, RibbonMenuItem>>>
        TopDockedGroupedItemsProperty =
            AvaloniaProperty.RegisterDirect<RibbonMenu, IEnumerable<IGrouping<string, RibbonMenuItem>>>(
                nameof(TopDockedGroupedItems),
                o => o.TopDockedGroupedItems);

    // AvaloniaProperty for BottomDockedGroupedItems, a collection of RibbonMenuItem groups
    public static readonly DirectProperty<RibbonMenu, IEnumerable<IGrouping<string, RibbonMenuItem>>>
        BottomDockedGroupedItemsProperty =
            AvaloniaProperty.RegisterDirect<RibbonMenu, IEnumerable<IGrouping<string, RibbonMenuItem>>>(
                nameof(BottomDockedGroupedItems),
                o => o.BottomDockedGroupedItems);

    public static readonly DirectProperty<RibbonMenu, ObservableCollection<RibbonRecentDocument>>
        RecentDocumentsProperty =
            AvaloniaProperty.RegisterDirect<RibbonMenu, ObservableCollection<RibbonRecentDocument>>(
                nameof(RecentDocuments),
                o => o.RecentDocuments,
                (o, v) => o.RecentDocuments = v);

    // Content Property for the RibbonMenu
    public static readonly StyledProperty<object?> ContentProperty =
        ContentControl.ContentProperty.AddOwner<RibbonMenu>();

    public static readonly StyledProperty<object?> LargeImageProperty =
        AvaloniaProperty.Register<RibbonMenu, object?>(nameof(LargeImage));

    public static readonly StyledProperty<object?> SmallImageProperty =
        AvaloniaProperty.Register<RibbonMenu, object?>(nameof(SmallImage));

    public static readonly StyledProperty<IBrush?> AccentBrushProperty =
        AvaloniaProperty.Register<RibbonMenu, IBrush?>(nameof(AccentBrush));

    public static readonly StyledProperty<bool> ShowDropDownArrowProperty =
        AvaloniaProperty.Register<RibbonMenu, bool>(nameof(ShowDropDownArrow), true);

    public static readonly StyledProperty<Thickness> DropDownArrowMarginProperty =
        AvaloniaProperty.Register<RibbonMenu, Thickness>(
            nameof(DropDownArrowMargin),
            new Thickness(0, 0, 4, 0));

    // AvaloniaProperty for the IsMenuOpen state
    public static readonly StyledProperty<bool> IsMenuOpenProperty =
        AvaloniaProperty.Register<RibbonMenu, bool>(nameof(IsMenuOpen));

    // AvaloniaProperty for SelectedItemContent
    public static readonly StyledProperty<object?> SelectedItemContentProperty =
        AvaloniaProperty.Register<RibbonMenu, object?>(nameof(SelectedItemContent));

    // AvaloniaProperty for SelectedSubItems
    public static readonly StyledProperty<object?> SelectedSubItemsProperty =
        AvaloniaProperty.Register<RibbonMenu, object?>(nameof(SelectedSubItems));

    // Default panel template used when no other template is specified
    private static readonly FuncTemplate<Panel> DefaultPanel = new(() => new StackPanel());

    // Private fields to hold the grouped items for top and bottom docks
    private IEnumerable<IGrouping<string, RibbonMenuItem>> _bottomDockedGroupedItems =
        Array.Empty<IGrouping<string, RibbonMenuItem>>();

    private IEnumerable<IGrouping<string, RibbonMenuItem>> _topDockedGroupedItems =
        Array.Empty<IGrouping<string, RibbonMenuItem>>();

    private ObservableCollection<RibbonRecentDocument> _recentDocuments = new();

    private Popup? _menuPopup;

    private Control? _contentButton;

    private Border? _menuRootBorder;

    static RibbonMenu()
    {
        IsMenuOpenProperty.Changed.AddClassHandler<RibbonMenu>((sender, e) =>
        {
            if (e.GetNewValue<bool>())
                sender.UpdatePopupLayout();
        });

        // Class handler for ItemsSourceProperty when it changes
        ItemsSourceProperty.Changed.AddClassHandler<RibbonMenu>((x, e) => x.ItemsChanged(e));
    }

    public RibbonMenu()
    {
        RecentDocumentClickCommand = new RelayCommand(ExecuteRecentDocumentCommand);
        Items.CollectionChanged += ItemsCollectionChanged;
    }

    // Public getter and setter for TopDockedGroupedItems
    public IEnumerable<IGrouping<string, RibbonMenuItem>> TopDockedGroupedItems
    {
        get => _topDockedGroupedItems;
        private set => SetAndRaise(TopDockedGroupedItemsProperty, ref _topDockedGroupedItems, value);
    }

    // Public getter and setter for BottomDockedGroupedItems
    public IEnumerable<IGrouping<string, RibbonMenuItem>> BottomDockedGroupedItems
    {
        get => _bottomDockedGroupedItems;
        private set => SetAndRaise(BottomDockedGroupedItemsProperty, ref _bottomDockedGroupedItems, value);
    }

    public ObservableCollection<RibbonRecentDocument> RecentDocuments
    {
        get => _recentDocuments;
        set => SetAndRaise(RecentDocumentsProperty, ref _recentDocuments, value);
    }

    // Public getter and setter for Content
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public object? LargeImage
    {
        get => GetValue(LargeImageProperty);
        set => SetValue(LargeImageProperty, value);
    }

    public object? SmallImage
    {
        get => GetValue(SmallImageProperty);
        set => SetValue(SmallImageProperty, value);
    }

    public IBrush? AccentBrush
    {
        get => GetValue(AccentBrushProperty);
        set => SetValue(AccentBrushProperty, value);
    }

    public bool ShowDropDownArrow
    {
        get => GetValue(ShowDropDownArrowProperty);
        set => SetValue(ShowDropDownArrowProperty, value);
    }

    public Thickness DropDownArrowMargin
    {
        get => GetValue(DropDownArrowMarginProperty);
        set => SetValue(DropDownArrowMarginProperty, value);
    }

    public ItemCollection LeftPaneItems => Items;

    // Public getter and setter for SelectedItemContent
    public object? SelectedItemContent
    {
        get => GetValue(SelectedItemContentProperty);
        set => SetValue(SelectedItemContentProperty, value);
    }

    // Public getter and setter for SelectedSubItems
    public object? SelectedSubItems
    {
        get => GetValue(SelectedSubItemsProperty);
        set => SetValue(SelectedSubItemsProperty, value);
    }

    // Public getter and setter for IsMenuOpen
    public bool IsMenuOpen
    {
        get => GetValue(IsMenuOpenProperty);
        set => SetValue(IsMenuOpenProperty, value);
    }

    public ICommand RecentDocumentClickCommand { get; }

    public event EventHandler<RibbonRecentDocument>? RecentDocumentInvoked;

    public event EventHandler<RibbonMenuItem>? ItemInvoked;

    // Constructor: Called when the template is applied
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _menuPopup = e.NameScope.Find<Popup>("MenuPopup");
        _contentButton = e.NameScope.Find<Control>("ContentButton");
        _menuRootBorder = e.NameScope.Find<Border>("MenuRootBorder");
        if (_menuPopup != null)
        {
            _menuPopup.Closed -= PopupOnClosed;
            _menuPopup.Closed += PopupOnClosed;
            _menuPopup.Opened -= Popup_Opened;
            _menuPopup.Opened += Popup_Opened;

            if (_contentButton != null)
            {
                _menuPopup.PlacementTarget = _contentButton;
                _menuPopup.OverlayInputPassThroughElement = _contentButton;
            }
        }

        // Update grouped items and reset item hover events
        UpdateGroupedItems();
        ResetItemHoverEvents();
    }

    /// <summary>
    ///     Handles the Popup Opened event.
    ///     Adjusts the Popup's position and size based on the top level window's size.
    /// </summary>
    private void Popup_Opened(object? sender, EventArgs e)
    {
        UpdatePopupLayout();
    }

    private void UpdatePopupLayout()
    {
        if (_menuPopup == null)
            return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null)
            return;

        var clientWidth = topLevel.ClientSize.Width;
        var clientHeight = topLevel.ClientSize.Height;
        if (clientWidth <= 0 || clientHeight <= 0)
            return;

        var ribbon = this.FindAncestorOfType<Ribbon>(true);
        var placementTarget = _contentButton ?? this;
        _menuPopup.PlacementTarget = placementTarget;

        var targetTopLeft = placementTarget.TranslatePoint(new Point(0, 0), topLevel);
        var targetLeft = targetTopLeft?.X ?? 0;
        var targetTop = targetTopLeft?.Y ?? 0;

        double width;
        double left;
        if (ribbon != null && ribbon.Orientation == Orientation.Horizontal && ribbon.Bounds.Width > 0)
        {
            width = Math.Min(ribbon.Bounds.Width, clientWidth);
            left = ribbon.TranslatePoint(new Point(0, 0), topLevel)?.X ?? 0;
        }
        else
        {
            width = clientWidth;
            left = 0;
        }

        var height = Math.Min(clientHeight - targetTop, clientHeight);
        if (width <= 0 || height <= 0)
            return;

        // Cover the client area from the File button row downward.
        // LeftEdgeAlignedTop places the popup beside the button and pushes it off-screen at the left edge.
        _menuPopup.Placement = PlacementMode.BottomEdgeAlignedLeft;
        _menuPopup.HorizontalOffset = left - targetLeft;
        _menuPopup.VerticalOffset = -(placementTarget.Bounds.Height);
        _menuPopup.Width = width;
        _menuPopup.Height = height;

        if (_menuRootBorder != null)
        {
            _menuRootBorder.Width = width;
            _menuRootBorder.Height = height;
            _menuRootBorder.MaxWidth = width;
            _menuRootBorder.MaxHeight = height;
        }
    }

    /// <summary>
    ///     Handles the Popup Closed event.
    /// </summary>
    private void PopupOnClosed(object? sender, EventArgs e)
    {
    }

    // Called when the items collection changes
    private void ItemsChanged(AvaloniaPropertyChangedEventArgs args)
    {
        UpdateGroupedItems();
        ResetItemHoverEvents();

        // Unsubscribe from old collection changes, if applicable
        if (args.OldValue is INotifyCollectionChanged oldSource)
            oldSource.CollectionChanged -= ItemsCollectionChanged;
        if (args.NewValue is INotifyCollectionChanged newSource)
            newSource.CollectionChanged += ItemsCollectionChanged;
    }

    // Resets item hover events for each item
    private void ResetItemHoverEvents()
    {
        foreach (var item in Items.OfType<RibbonMenuItem>())
        {
            item.Click -= Item_Clicked;
            item.Click += Item_Clicked;
        }
    }

    /// <summary>
    ///     Handles the Item Clicked event.
    ///     Updates the selected item content based on the clicked item.
    /// </summary>
    private void Item_Clicked(object? sender, RoutedEventArgs e)
    {
        var item = sender as RibbonMenuItem;
        if (item == null) return;

        SelectedItemContent = item.Content;
        ItemInvoked?.Invoke(this, item);
    }

    private void ExecuteRecentDocumentCommand(object? parameter)
    {
        if (parameter is not RibbonRecentDocument recentDocument)
            return;

        var commandParameter = recentDocument.CommandParameter ?? recentDocument;
        if (recentDocument.Command != null && recentDocument.Command.CanExecute(commandParameter))
            recentDocument.Command.Execute(commandParameter);

        RecentDocumentInvoked?.Invoke(this, recentDocument);
    }

    // Updates grouped items based on top-docked and bottom-docked criteria
    private void UpdateGroupedItems()
    {
        // Group items for the TopDocked section
        TopDockedGroupedItems = Items.OfType<RibbonMenuItem>()
            .Where(x => x.IsTopDocked && !x.IsBottomDocked)
            .GroupBy(x => string.IsNullOrWhiteSpace(x.Group) ? "Ungrouped" : x.Group)
            .ToList();

        // Group items for the BottomDocked section
        BottomDockedGroupedItems = Items.OfType<RibbonMenuItem>()
            .Where(x => x.IsBottomDocked)
            .GroupBy(x => string.IsNullOrWhiteSpace(x.Group) ? "Ungrouped" : x.Group)
            .ToList();

        // Set flags for the last item in each group
        try
        {
            SetIsLastItemFlag(TopDockedGroupedItems, false); // Top docked groups
            SetIsLastItemFlag(BottomDockedGroupedItems, true); // Bottom docked groups
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    // Resets the selection state for all items
    private void ResetSelection()
    {
        foreach (var item in Items.OfType<RibbonMenuItem>()) item.IsSelected = false;
    }

    // Handles collection changes for the items
    private void ItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateGroupedItems();
        ResetItemHoverEvents();
    }

    // Sets the IsLastItem flag for items in the grouped collection
    private void SetIsLastItemFlag(IEnumerable<IGrouping<string, RibbonMenuItem>> groupedItems, bool isBottomDocked)
    {
        var groupList = groupedItems.ToList();

        // Iterate over each group
        for (var groupIndex = 0; groupIndex < groupList.Count; groupIndex++)
        {
            var group = groupList[groupIndex];
            var itemList = group.ToList();

            // Set the IsLastItem flag for each item in the group
            for (var itemIndex = 0; itemIndex < itemList.Count; itemIndex++)
                itemList[itemIndex].IsLastItem = itemIndex == itemList.Count - 1;

            // If it's the last group and it's in the bottom docked section, hide the group
            if (isBottomDocked && groupIndex == groupList.Count - 1)
                foreach (var item in itemList)
                    item.IsLastItem = false; // Set visibility flag for the last group in the bottom dock
        }
    }

    private sealed class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;

        public RelayCommand(Action<object?> execute)
        {
            _execute = execute;
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
