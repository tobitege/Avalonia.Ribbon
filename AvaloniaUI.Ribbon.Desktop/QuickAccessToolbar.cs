using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using AvaloniaUI.Ribbon.Contracts;

namespace AvaloniaUI.Ribbon.Desktop;

[TemplatePart("PART_MoreButton", typeof(ToggleButton))]
public class QuickAccessToolbar : ItemsControl, INotifyPropertyChanged //, IKeyTipHandler
{
    public event EventHandler<ICanAddToQuickAccess?>? ItemAdded;

    public event EventHandler<ICanAddToQuickAccess?>? ItemRemoved;

    public static bool GetIsChecked(MenuItem element)
    {
        return element.GetValue(IsCheckedProperty);
    }

    public static void SetIsChecked(MenuItem element, bool value)
    {
        element.SetValue(IsCheckedProperty, value);
    }

    public static string? GetPersistenceId(AvaloniaObject element)
    {
        return element.GetValue(PersistenceIdProperty);
    }

    public static void SetPersistenceId(AvaloniaObject element, string? value)
    {
        element.SetValue(PersistenceIdProperty, value);
    }

    public bool AddItem(ICanAddToQuickAccess? item)
    {
        var contains = ContainsItem(item, out _);
        if (item == null || contains)
            return false;

        if (ItemsSource is not ObservableCollection<QuickAccessItem> itemsSource)
            return false;

        if (item.CanAddToQuickAccess)
        {
            RegisterAvailableItem(item);
            itemsSource.Add(new QuickAccessItem { Item = item });
            ItemAdded?.Invoke(this, item);
            SaveState();
            return true;
        }

        return false;
    }

    public bool ContainsItem(ICanAddToQuickAccess? item)
    {
        return ContainsItem(item, out _);
    }

    public bool ContainsItem(ICanAddToQuickAccess? item, out object? result)
    {
        if (item == null)
        {
            result = null;
            return false;
        }

        if (Items.OfType<ICanAddToQuickAccess>().Contains(item))
        {
            result = item;
            return true;
        }

        if (Items.OfType<QuickAccessItem>().Any(x => x.Item == item))
        {
            result = Items.OfType<QuickAccessItem>().First(x => x.Item == item);
            return true;
        }

        result = null;
        return false;
    }

    public void MoreFlyoutMenuItemCommand(object? parameter)
    {
        if (parameter is ICanAddToQuickAccess item)
        {
            if (!AddItem(item))
                RemoveItem(item);
        }
        else if (parameter is Action cmd)
        {
            cmd();
        }
    }

    public bool RemoveItem(ICanAddToQuickAccess? item)
    {
        var contains = ContainsItem(item, out var result);
        if (item == null || !contains)
            return false;

        if (ItemsSource is not ObservableCollection<QuickAccessItem> itemsSource)
            return false;

        var match = result as QuickAccessItem;
        if (match == null)
        {
            match = itemsSource.FirstOrDefault(x => x.Item == item);
            if (match == null)
                return false;
        }

        var removed = itemsSource.Remove(match);
        if (removed)
        {
            ItemRemoved?.Invoke(this, item);
            SaveState();
        }

        return removed;
    }

    public void SyncItems(IEnumerable<ICanAddToQuickAccess> items)
    {
        if (ItemsSource is not ObservableCollection<QuickAccessItem> itemsSource)
            return;

        itemsSource.Clear();

        foreach (var item in items.Distinct())
            if (item != null && item.CanAddToQuickAccess)
            {
                RegisterAvailableItem(item);
                itemsSource.Add(new QuickAccessItem { Item = item });
            }

        SaveState();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var more = e.NameScope.Find<ToggleButton>("PART_MoreButton");
        if (more is null)
            return;

        _moreButton = more;
        _moreButton.IsVisible = ShowOverflowButton;

        var moreCmdItem = new MenuItem();
        moreCmdItem.Classes.Add(FIXED_ITEM_CLASS);
        moreCmdItem[!HeaderedSelectingItemsControl.HeaderProperty] =
            moreCmdItem.GetResourceObservable("AvaloniaRibbon.MoreQATCommands").ToBinding();
        moreCmdItem[!MenuItem.CommandProperty] = this[!MoreCommandsCommandProperty];
        moreCmdItem[!MenuItem.CommandParameterProperty] = this[!MoreCommandsCommandParameterProperty];
        moreCmdItem[!IsVisibleProperty] = this.GetObservable(MoreCommandsCommandProperty)
            .Select(command => command != null)
            .ToBinding();

        var morCtx = more.ContextMenu;

        if (morCtx is null)
            return;
        morCtx.Opened += (sneder, a) =>
        {
            if (more.IsChecked != true)
                more.IsChecked = true;

            ObservableCollection<object> morCtxItems = new();
            foreach (var rcm in RecommendedItems)
            {
                rcm.IsChecked = ContainsItem(rcm.Item);
                morCtxItems.Add(rcm);
            }

            morCtxItems.Add(new Separator());
            morCtxItems.Add(moreCmdItem);
            morCtxItems.Add(_collapseRibbonItem);
            morCtx.ItemsSource = morCtxItems;
        };

        morCtx.Closed += (sender, a) =>
        {
            if (more.IsChecked == true)
                more.IsChecked = false;
        };
        more.IsCheckedChanged += delegate
        {
            if (more.IsChecked == true)
                morCtx.Open(more);
            else if (more.IsChecked == false) morCtx.Close();
        };

        RestoreState();
    }

    #region Fields

    public static readonly AttachedProperty<bool> IsCheckedProperty =
        AvaloniaProperty.RegisterAttached<QuickAccessToolbar, MenuItem, bool>("IsChecked");

    public static readonly AttachedProperty<string?> PersistenceIdProperty =
        AvaloniaProperty.RegisterAttached<QuickAccessToolbar, AvaloniaObject, string?>("PersistenceId");

    public static readonly DirectProperty<QuickAccessToolbar, bool> ShowOverflowButtonProperty =
        AvaloniaProperty.RegisterDirect<QuickAccessToolbar, bool>(nameof(ShowOverflowButton),
            o => o.ShowOverflowButton,
            (o, v) => o.ShowOverflowButton = v);

    public static readonly StyledProperty<bool> ShowItemsProperty =
        AvaloniaProperty.Register<QuickAccessToolbar, bool>(nameof(ShowItems), true);

    public static readonly StyledProperty<ICommand?> MoreCommandsCommandProperty =
        AvaloniaProperty.Register<QuickAccessToolbar, ICommand?>(nameof(MoreCommandsCommand));

    public static readonly StyledProperty<object?> MoreCommandsCommandParameterProperty =
        AvaloniaProperty.Register<QuickAccessToolbar, object?>(nameof(MoreCommandsCommandParameter));

    public static readonly StyledProperty<IQuickAccessToolbarPersistenceProvider?> PersistenceProviderProperty =
        AvaloniaProperty.Register<QuickAccessToolbar, IQuickAccessToolbarPersistenceProvider?>(
            nameof(PersistenceProvider));

    public static readonly StyledProperty<string?> PersistenceKeyProperty =
        AvaloniaProperty.Register<QuickAccessToolbar, string?>(nameof(PersistenceKey));

    public static readonly DirectProperty<QuickAccessToolbar, ObservableCollection<ICanAddToQuickAccess>>
        AvailableItemsProperty =
            AvaloniaProperty.RegisterDirect<QuickAccessToolbar, ObservableCollection<ICanAddToQuickAccess>>(
                nameof(AvailableItems), o => o.AvailableItems, (o, v) => o.AvailableItems = v);

    public static readonly DirectProperty<QuickAccessToolbar, ObservableCollection<QuickAccessRecommendation>>
        RecommendedItemsProperty =
            AvaloniaProperty.RegisterDirect<QuickAccessToolbar, ObservableCollection<QuickAccessRecommendation>>(
                nameof(RecommendedItems), o => o.RecommendedItems, (o, v) => o.RecommendedItems = v);

    public static readonly StyledProperty<DesktopRibbon?> RibbonProperty =
        AvaloniaProperty.Register<QuickAccessToolbar, DesktopRibbon?>(nameof(Ribbon));

    private static readonly string FIXED_ITEM_CLASS = "quickAccessFixedItem";

    private readonly MenuItem _collapseRibbonItem = new();

    private ToggleButton? _moreButton;

    private ObservableCollection<ICanAddToQuickAccess> _availableItems = new();

    private ObservableCollection<QuickAccessRecommendation> _recommendedItems = new();

    private bool _showOverflowButton = true;

    private bool _isRestoringState;

    private bool _persistenceInitialized;

    #endregion Fields

    #region Constructors

    static QuickAccessToolbar()
    {
        ShowOverflowButtonProperty.Changed.AddClassHandler<QuickAccessToolbar>((sender, e) =>
            sender.UpdateOverflowButtonVisibility());

        PersistenceProviderProperty.Changed.AddClassHandler<QuickAccessToolbar>((sender, e) =>
            sender.RestoreState());
        PersistenceKeyProperty.Changed.AddClassHandler<QuickAccessToolbar>((sender, e) =>
            sender.RestoreState());

        RibbonProperty.Changed.AddClassHandler<QuickAccessToolbar>((sender, e) =>
        {
            if (sender.Ribbon != null)
                sender._collapseRibbonItem[!IsCheckedProperty] = sender.Ribbon[!DesktopRibbon.IsCollapsedProperty];
            else
                SetIsChecked(sender._collapseRibbonItem, false);
        });
    }

    public QuickAccessToolbar()
    {
        _collapseRibbonItem.Classes.Add(FIXED_ITEM_CLASS);
        _collapseRibbonItem[!HeaderedSelectingItemsControl.HeaderProperty] = _collapseRibbonItem
            .GetResourceObservable("AvaloniaRibbon.MinimizeRibbon").ToBinding();
        _collapseRibbonItem[!IsEnabledProperty] = this.GetObservable(RibbonProperty).Select(x => x != null).ToBinding();
        _collapseRibbonItem.Click += (_, _) =>
        {
            if (Ribbon != null)
                Ribbon.IsCollapsed = !Ribbon.IsCollapsed;
        };
        ItemsSource = new ObservableCollection<QuickAccessItem>();
    }

    #endregion Constructors

    #region Properties

    public ObservableCollection<QuickAccessRecommendation> RecommendedItems
    {
        get => _recommendedItems;
        set => SetAndRaise(RecommendedItemsProperty, ref _recommendedItems, value);
    }

    public ObservableCollection<ICanAddToQuickAccess> AvailableItems
    {
        get => _availableItems;
        set => SetAndRaise(AvailableItemsProperty, ref _availableItems, value);
    }

    public bool ShowOverflowButton
    {
        get => _showOverflowButton;
        set => SetAndRaise(ShowOverflowButtonProperty, ref _showOverflowButton, value);
    }

    public bool ShowItems
    {
        get => GetValue(ShowItemsProperty);
        set => SetValue(ShowItemsProperty, value);
    }

    public ICommand? MoreCommandsCommand
    {
        get => GetValue(MoreCommandsCommandProperty);
        set => SetValue(MoreCommandsCommandProperty, value);
    }

    public object? MoreCommandsCommandParameter
    {
        get => GetValue(MoreCommandsCommandParameterProperty);
        set => SetValue(MoreCommandsCommandParameterProperty, value);
    }

    public IQuickAccessToolbarPersistenceProvider? PersistenceProvider
    {
        get => GetValue(PersistenceProviderProperty);
        set => SetValue(PersistenceProviderProperty, value);
    }

    public string? PersistenceKey
    {
        get => GetValue(PersistenceKeyProperty);
        set => SetValue(PersistenceKeyProperty, value);
    }

    public DesktopRibbon? Ribbon
    {
        get => GetValue(RibbonProperty);
        set => SetValue(RibbonProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(QuickAccessToolbar);

    #endregion Properties

    private void UpdateOverflowButtonVisibility()
    {
        if (_moreButton != null)
            _moreButton.IsVisible = ShowOverflowButton;
    }

    public bool RestoreState()
    {
        if (PersistenceProvider == null || string.IsNullOrWhiteSpace(PersistenceKey))
            return false;

        var itemIds = PersistenceProvider.Load(PersistenceKey);
        _persistenceInitialized = true;
        if (itemIds == null)
        {
            SaveState();
            return false;
        }

        var availableItems = AvailableItems
            .Concat(RecommendedItems.Select(recommendation => recommendation.Item))
            .Where(item => item != null)
            .Distinct()
            .Select(item => new { Id = GetItemPersistenceId(item), Item = item })
            .Where(registration => !string.IsNullOrWhiteSpace(registration.Id))
            .ToDictionary(registration => registration.Id, registration => registration.Item);

        _isRestoringState = true;
        try
        {
            foreach (var item in Items.OfType<QuickAccessItem>().Select(container => container.Item).ToArray())
                RemoveItem(item);

            foreach (var itemId in itemIds)
                if (availableItems.TryGetValue(itemId, out var item))
                    AddItem(item);
        }
        finally
        {
            _isRestoringState = false;
        }

        return true;
    }

    public void SaveState()
    {
        if (_isRestoringState || !_persistenceInitialized || PersistenceProvider == null ||
            string.IsNullOrWhiteSpace(PersistenceKey))
            return;

        var itemIds = Items
            .OfType<QuickAccessItem>()
            .Select(container => GetItemPersistenceId(container.Item))
            .Where(itemId => !string.IsNullOrWhiteSpace(itemId))
            .ToArray();

        PersistenceProvider.Save(PersistenceKey, itemIds);
    }

    private void RegisterAvailableItem(ICanAddToQuickAccess item)
    {
        if (!AvailableItems.Contains(item))
            AvailableItems.Add(item);
    }

    private static string GetItemPersistenceId(ICanAddToQuickAccess item)
    {
        return item is AvaloniaObject avaloniaObject
            ? GetPersistenceId(avaloniaObject) ?? string.Empty
            : string.Empty;
    }

    /*protected override void ItemsChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.ItemsChanged(e);
        RefreshItems();
    }

    protected override void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        base.ItemsCollectionChanged(sender, e);
        RefreshItems();
    }

    void RefreshItems()
    {
        panel.Children.Clear();

        foreach (Control itm in ((AvaloniaList<object>)Items).OfType<Control>())
            panel.Children.Add(itm);
    }*/

    /*private protected override ItemContainerGenerator CreateItemContainerGenerator()
    {
        return new ItemContainerGenerator<QuickAccessItem>(this, QuickAccessItem.ItemProperty, QuickAccessItem.ContentTemplateProperty);
    }*/
}
