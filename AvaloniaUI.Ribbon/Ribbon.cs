using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using AvaloniaUI.Ribbon.Contracts;
using AvaloniaUI.Ribbon.Models;

namespace AvaloniaUI.Ribbon;

[TemplatePart("PART_CollapsedContentPopup", typeof(Popup))]
[TemplatePart("PART_SelectedGroupsHost", typeof(ItemsControl))]
[TemplatePart("PART_GroupsPresenterHolder", typeof(ContentControl))]
[TemplatePart("PART_PopupGroupsPresenterHolder", typeof(ContentControl))]
[TemplatePart("PART_ItemsPresenter", typeof(ItemsPresenter))]
[TemplatePart("PART_PinLastHoveredControlToQuickAccess", typeof(MenuItem))]
[TemplatePart("PART_ContentAreaContextMenu", typeof(ContextMenu))]
[TemplatePart("PART_CollapseRibbon", typeof(MenuItem))]
public class Ribbon : TabControl, IRibbon
{
    static Ribbon()
    {
        TabsProperty.Changed.AddClassHandler<Ribbon>((ribbon, args) =>
        {
            if (ribbon.Tabs.Count > 0)
                ribbon.SelectedIndex = 1;
            else
                ribbon.SelectedIndex = 0;
            ribbon.RefreshTabs();
        });

        OrientationProperty.OverrideDefaultValue<Ribbon>(Orientation.Horizontal);

        SelectedIndexProperty.Changed.AddClassHandler<Ribbon>((x, e) => x.RefreshSelectedGroups());

        IsCollapsedProperty.Changed.AddClassHandler<Ribbon, bool>((sender, args) =>
        {
            if (sender.IsCollapsedPopupOpen)
                sender.IsCollapsedPopupOpen = false;

            sender.UpdatePresenterLocation(args.NewValue.Value);
        });

        KeyTip.ShowChildKeyTipKeysProperty.Changed.AddClassHandler<Ribbon>((sender, args) =>
        {
            if (args.NewValue is not bool isOpen)
                return;

            if (isOpen)
                sender.Focus();
            sender.SetChildKeyTipsVisibility(isOpen);
        });

        LostFocusEvent.AddClassHandler<Ribbon>((sender, _) => KeyTip.SetShowChildKeyTipKeys(sender, false));
    }

    public Ribbon()
    {
        ContextualTabGroups = new ObservableCollection<RibbonContextualTabGroup>();
        QuickAccessItems = new QuickAccessItemsCollection();
    }

    protected override Type StyleKeyOverride => typeof(Ribbon);

    #region Static Properties

    public static readonly StyledProperty<IBrush> HeaderBackgroundProperty =
        AvaloniaProperty.Register<Ribbon, IBrush>(nameof(HeaderBackground));

    public static readonly StyledProperty<IBrush> HeaderForegroundProperty =
        AvaloniaProperty.Register<Ribbon, IBrush>(nameof(HeaderForeground));

    public static readonly StyledProperty<object> HelpPaneContentProperty =
        AvaloniaProperty.Register<Ribbon, object>(nameof(HelpPaneContent));

    public static readonly StyledProperty<IList<RibbonContextualTabGroup>> ContextualTabGroupsProperty =
        AvaloniaProperty.Register<Ribbon, IList<RibbonContextualTabGroup>>(nameof(ContextualTabGroups),
            defaultBindingMode: BindingMode.OneWay);

    public IList<RibbonContextualTabGroup> ContextualTabGroups
    {
        get => GetValue(ContextualTabGroupsProperty);
        set => SetValue(ContextualTabGroupsProperty, value);
    }

    public static readonly StyledProperty<bool> IsCollapsedPopupOpenProperty =
        AvaloniaProperty.Register<Ribbon, bool>(nameof(IsCollapsedPopupOpen));

    public static readonly StyledProperty<bool> IsCollapsedProperty =
        AvaloniaProperty.Register<Ribbon, bool>(nameof(IsCollapsed));

    public static readonly DirectProperty<MenuBase, bool> IsOpenProperty =
        AvaloniaProperty.RegisterDirect<MenuBase, bool>(nameof(IsOpen), (Func<MenuBase, bool>)(o => o.IsOpen));

    public static readonly RoutedEvent<RoutedEventArgs> MenuClosedEvent =
        RoutedEvent.Register<Ribbon, RoutedEventArgs>(nameof(MenuClosed), RoutingStrategies.Bubble);

    public static readonly StyledProperty<IRibbonMenu?> MenuProperty =
        AvaloniaProperty.Register<Ribbon, IRibbonMenu?>(nameof(Menu));

    public static readonly StyledProperty<Orientation> OrientationProperty =
        StackPanel.OrientationProperty.AddOwner<Ribbon>();

    public static readonly StyledProperty<RibbonGroupOverflowBehavior> GroupOverflowBehaviorProperty =
        AvaloniaProperty.Register<Ribbon, RibbonGroupOverflowBehavior>(nameof(GroupOverflowBehavior),
            RibbonGroupOverflowBehavior.ShrinkOnly);

    public static readonly StyledProperty<RibbonQatLocation> QuickAccessLocationProperty =
        AvaloniaProperty.Register<Ribbon, RibbonQatLocation>(nameof(QuickAccessLocation),
            RibbonQatLocation.Above);

    public static readonly StyledProperty<int> MaxGroupRowsProperty =
        AvaloniaProperty.Register<Ribbon, int>(nameof(MaxGroupRows), 1,
            coerce: (_, value) => Math.Max(1, value));

    public static readonly RoutedEvent<RoutedEventArgs> RibbonKeyTipsOpenedEvent =
        RoutedEvent.Register<MenuBase, RoutedEventArgs>("RibbonKeyTipsOpened", RoutingStrategies.Bubble);

    public static readonly DirectProperty<Ribbon, ObservableCollection<RibbonGroupBox>> SelectedGroupsProperty =
        AvaloniaProperty.RegisterDirect<Ribbon, ObservableCollection<RibbonGroupBox>>(nameof(SelectedGroups),
            o => o.SelectedGroups, (o, v) => o.SelectedGroups = v);

    public static readonly DirectProperty<Ribbon, ObservableCollection<Control>> TabsProperty =
        AvaloniaProperty.RegisterDirect<Ribbon, ObservableCollection<Control>>(nameof(Tabs), o => o.Tabs,
            (o, v) => o.Tabs = v);

    public static readonly DirectProperty<Ribbon, bool> ShowQatOverflowButtonProperty =
        AvaloniaProperty.RegisterDirect<Ribbon, bool>(nameof(ShowQatOverflowButton), o => o.ShowQatOverflowButton,
            (o, v) => o.ShowQatOverflowButton = v);

    public static readonly DirectProperty<Ribbon, ObservableCollection<ICanAddToQuickAccess>> QuickAccessItemsProperty =
        AvaloniaProperty.RegisterDirect<Ribbon, ObservableCollection<ICanAddToQuickAccess>>(
            nameof(QuickAccessItems), o => o.QuickAccessItems, (o, v) => o.QuickAccessItems = v);

    #endregion Static Properties

    #region Fields

    protected ContextMenu? _ctxMenu;

    private ContentControl? _flyoutPresenter;

    protected ItemsControl? _groupsHost;

    private bool _isOpen;

    private ItemsPresenter? _itemHeadersPresenter;

    private ContentControl? _mainPresenter;

    private Popup? _popup;

    private IInputElement? _prevFocusedElement;

    private RibbonTab? _prevSelectedTab;

    protected ICanAddToQuickAccess? _rightClicked;

    private ObservableCollection<RibbonGroupBox> _selectedGroups = new();

    private ObservableCollection<Control> _tabs = new();

    private bool _showQatOverflowButton = true;

    private ObservableCollection<ICanAddToQuickAccess> _quickAccessItems = new QuickAccessItemsCollection();

    #endregion Fields

    #region Properties

    public event EventHandler<RoutedEventArgs> MenuClosed
    {
        add => AddHandler(MenuClosedEvent, value);
        remove => RemoveHandler(MenuClosedEvent, value);
    }

    public IBrush HeaderBackground
    {
        get => GetValue(HeaderBackgroundProperty);
        set => SetValue(HeaderBackgroundProperty, value);
    }

    public IBrush HeaderForeground
    {
        get => GetValue(HeaderForegroundProperty);
        set => SetValue(HeaderForegroundProperty, value);
    }

    public object HelpPaneContent
    {
        get => GetValue(HelpPaneContentProperty);
        set => SetValue(HelpPaneContentProperty, value);
    }

    public bool IsCollapsed
    {
        get => GetValue(IsCollapsedProperty);
        set => SetValue(IsCollapsedProperty, value);
    }

    public bool IsCollapsedPopupOpen
    {
        get => GetValue(IsCollapsedPopupOpenProperty);
        set => SetValue(IsCollapsedPopupOpenProperty, value);
    }

    public bool IsOpen
    {
        get => _isOpen;
        protected set => SetAndRaise(MenuBase.IsOpenProperty, ref _isOpen, value);
    }

    public IRibbonMenu? Menu
    {
        get => GetValue(MenuProperty);
        set => SetValue(MenuProperty, value);
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public RibbonGroupOverflowBehavior GroupOverflowBehavior
    {
        get => GetValue(GroupOverflowBehaviorProperty);
        set => SetValue(GroupOverflowBehaviorProperty, value);
    }

    public RibbonQatLocation QuickAccessLocation
    {
        get => GetValue(QuickAccessLocationProperty);
        set => SetValue(QuickAccessLocationProperty, value);
    }

    public int MaxGroupRows
    {
        get => GetValue(MaxGroupRowsProperty);
        set => SetValue(MaxGroupRowsProperty, value);
    }

    public ObservableCollection<RibbonGroupBox> SelectedGroups
    {
        get => _selectedGroups;
        set => SetAndRaise(SelectedGroupsProperty, ref _selectedGroups, value);
    }

    public ObservableCollection<Control> Tabs
    {
        get => _tabs;
        set => SetAndRaise(TabsProperty, ref _tabs, value);
    }

    public bool ShowQatOverflowButton
    {
        get => _showQatOverflowButton;
        set => SetAndRaise(ShowQatOverflowButtonProperty, ref _showQatOverflowButton, value);
    }

    public ObservableCollection<ICanAddToQuickAccess> QuickAccessItems
    {
        get => _quickAccessItems;
        set => SetAndRaise(QuickAccessItemsProperty, ref _quickAccessItems, value);
    }

    #endregion Properties

    #region Methods

    public void ActivateKeyTips(IRibbon ribbon, IKeyTipHandler prev)
    {
        foreach (var t in Items.OfType<RibbonTab>())
            KeyTip.GetKeyTipKeys(t);

        if (Menu is Control menuControl)
            KeyTip.GetKeyTipKeys(menuControl);
    }

    public void Close()
    {
        if (!IsOpen)
            return;

        KeyTip.SetShowChildKeyTipKeys(this, false);
        IsOpen = false;
        _prevFocusedElement?.Focus();

        RaiseEvent(new RoutedEventArgs
        {
            RoutedEvent = MenuClosedEvent,
            Source = this
        });
    }

    public void CycleTabs(bool forward)
    {
        var switchTabs = false;
        //var tabs = ((AvaloniaList<object>)Items).OfType<RibbonTab>().Where(x => x.IsEffectivelyVisible && x.IsEnabled);
        var newIndex = SelectedIndex;
        Action stepIndex;
        Func<bool> verifyIndex;

        if (forward)
        {
            stepIndex = () => newIndex++;
            verifyIndex = () => newIndex < ItemCount - 1;
        }
        else
        {
            stepIndex = () => newIndex--;
            verifyIndex = () => newIndex > 0;
        }

        /*while (newIndex < ((AvaloniaList<object>)Items).Count)
        {
            step();
            RibbonTab newSel = (RibbonTab)(((AvaloniaList<object>)Items).ElementAt(newIndex));
            bool contextualVisible = true;
            if (newSel.IsContextual)
                contextualVisible = (newSel.Parent as RibbonContextualTabGroup).IsVisible;
            if (newSel.IsVisible && newSel.IsEnabled && contextualVisible)
            {
                SelectedIndex = newIndex;
                break;
            }
        }*/
        while (verifyIndex())
        {
            stepIndex();
            var newTab = Items.OfType<RibbonTab>().ElementAt(newIndex);

            var contextualVisible = true;
            if (newTab.IsContextual)
            {
                if (newTab.Parent is RibbonContextualTabGroup contextualGroup)
                    contextualVisible = contextualGroup.IsVisible;
                else
                    contextualVisible = false;
            }
            if (newTab.IsEffectivelyVisible && newTab.IsEnabled && contextualVisible)
            {
                switchTabs = true;
                break;
            }
        }

        if (switchTabs)
            SelectedIndex = newIndex;
    }

    public void GoToPreviousTab()
    {
        throw new NotImplementedException();
        //var tabs = ((AvaloniaList<object>)Items).OfType<RibbonTab>().Where(x => x.IsEffectivelyVisible && x.IsEnabled);
    }

    public bool HandleKeyTipKeyPress(Key key)
    {
        var retVal = false;
        if (IsOpen)
        {
            var tabKeyMatched = false;
            foreach (var t in Items.OfType<RibbonTab>())
                if (KeyTip.HasKeyTipKey(t, key))
                {
                    SelectedItem = t;
                    tabKeyMatched = true;
                    retVal = true;
                    if (IsCollapsed)
                        IsCollapsedPopupOpen = true;
                    t.ActivateKeyTips(this, this);
                    break;
                }

            if (!tabKeyMatched && Menu is Control menuControl)
                if (KeyTip.HasKeyTipKey(menuControl, key))
                {
                    Menu.IsMenuOpen = true;
                    if (Menu is IKeyTipHandler handler) handler.ActivateKeyTips(this, this);
                    retVal = true;
                }
        }

        return retVal;
    }

    public void Open()
    {
        if (IsOpen)
            return;

        IsOpen = true;
        if (VisualRoot is TopLevel topLevel)
            _prevFocusedElement = topLevel.FocusManager?.GetFocusedElement();
        Focus();
        KeyTip.SetShowChildKeyTipKeys(this, true);

        RaiseEvent(new RoutedEventArgs
        {
            RoutedEvent = RibbonKeyTipsOpenedEvent,
            Source = this
        });
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _popup = e.NameScope.Find<Popup>("PART_CollapsedContentPopup");
        if (_popup != null)
        {
            _popup.Opened -= OnCollapsedRibbon_Open;
            _popup.Opened += OnCollapsedRibbon_Open;
        }

        _groupsHost = e.NameScope.Find<ItemsControl>("PART_SelectedGroupsHost");
        _mainPresenter = e.NameScope.Find<ContentControl>("PART_GroupsPresenterHolder");
        _flyoutPresenter = e.NameScope.Find<ContentControl>("PART_PopupGroupsPresenterHolder");

        _itemHeadersPresenter = e.NameScope.Find<ItemsPresenter>("PART_ItemsPresenter");

        UpdatePresenterLocation(IsCollapsed);

        var secondClick = false;
        if (_itemHeadersPresenter is not null)
            _itemHeadersPresenter.PointerReleased += (_, _) =>
            {
                if (IsCollapsed)
                {
                    RibbonTab? mouseOverItem = null;
                    foreach (var tab in Items.OfType<RibbonTab>())
                        if (tab.IsPointerOver)
                        {
                            mouseOverItem = tab;
                            break;
                        }

                    if (mouseOverItem != null)
                    {
                        if (SelectedItem != mouseOverItem)
                            SelectedItem = mouseOverItem;
                        if (!secondClick)
                            IsCollapsedPopupOpen = true;
                        else
                            secondClick = false;
                    }
                }
                else
                {
                    foreach (var tab in Items.OfType<RibbonTab>())
                        if (tab.IsPointerOver && !tab.IsContextual)
                        {
                            SelectedItem = tab;
                            break;
                        }
                }
            };
        /*_itemHeadersPresenter.DoubleTapped += (sneder, args) =>
        {
            if (IsCollapsed)
            {
                if (IsCollapsedPopupOpen)
                    IsCollapsedPopupOpen = false;
                IsCollapsed = false;
            }
            else
            {
                IsCollapsed = true;
                secondClick = true;
            }
        };*/

        _ctxMenu = e.NameScope.Find<ContextMenu>("PART_ContentAreaContextMenu");

        var collapseRibbon = e.NameScope.Find<MenuItem>("PART_CollapseRibbon");
        if (collapseRibbon is not null)
            collapseRibbon.Click += (_, _) =>
            {
                if (IsCollapsed)
                    IsCollapsedPopupOpen = false;

                IsCollapsed = !IsCollapsed;
            };
        if (_groupsHost is not null)
        {
            _groupsHost.PointerExited += (_, _) =>
            {
                if (_ctxMenu == null || !_ctxMenu.IsOpen)
                    _rightClicked = null;
            };
            _groupsHost.AddHandler(PointerReleasedEvent,
                (_, args) => { }, handledEventsToo: true);
        }

        /*if (_popup is { })
        {
            _popup.LostFocus += (_, _) =>
            {
                if (IsOpen)
                {
                    Close();
                }
            };
        }*/
    }

    /// <summary>
    ///     Handle ribbon collapsed popup open
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnCollapsedRibbon_Open(object? sender, EventArgs e)
    {
        var popup = sender as Popup;
        if (popup == null) return;

        Console.Write(popup.HorizontalOffset);
        Console.Write(popup.VerticalOffset);
        Console.Write(popup.PlacementRect);
        Console.Write(popup.PlacementTarget);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is WindowBase wnd)
            wnd.Deactivated += InputRoot_Deactivated;
        topLevel?.AddHandler(PointerPressedEvent, InputRoot_PointerPressed, handledEventsToo: true);

        RefreshTabs();
        RefreshSelectedGroups();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is WindowBase wnd)
            wnd.Deactivated -= InputRoot_Deactivated;
        topLevel?.RemoveHandler(PointerPressedEvent, InputRoot_PointerPressed);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (IsFocused || IsPointerOver)
        {
            if (TryHandleShortcut(e.Key, e.KeyModifiers))
            {
                e.Handled = true;
                return;
            }

            KeyTip.SetShowChildKeyTipKeys(this, false);

            if (!IsOpen)
                Open();
            else if (e.Key == Key.Escape)
            {
                if (!TryNavigateBackFromTabKeyTips())
                    Close();
            }
            else if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt || e.Key == Key.F10)
            {
                Close();
            }
            else
                HandleKeyTipKeyPress(e.Key);
        }
    }

    private void HandleKeyTipControl(Control item)
    {
        item.RaiseEvent(new RoutedEventArgs(PointerPressedEvent));
        item.RaiseEvent(new RoutedEventArgs(PointerReleasedEvent));
    }

    private void InputRoot_Deactivated(object? sender, EventArgs e)
    {
        Close();
    }

    private void InputRoot_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (IsCollapsedPopupOpen && _groupsHost?.IsPointerOver != true)
            IsCollapsedPopupOpen = false;
    }

    protected void RefreshSelectedGroups()
    {
        SelectedGroups.Clear();
        if (_prevSelectedTab != null)
        {
            _prevSelectedTab.IsSelected = false;
            _prevSelectedTab = null;
        }

        if (SelectedItem != null && SelectedItem is RibbonTab tab)
        {
            foreach (var box in tab.Groups)
                SelectedGroups.Add(box);

            if (tab.IsContextual)
            {
                tab.IsSelected = true;
                _prevSelectedTab = tab;
            }
        }
    }

    protected internal void RefreshTabs()
    {
        if (Tabs is not null)
        {
            if (ItemsSource is IList list)
            {
                list.Clear();
                foreach (var ctrl in Tabs)
                    if (ctrl is RibbonContextualTabGroup ctx)
                        foreach (var tb in ctx.Items.OfType<RibbonTab>())
                            list.Add(tb);
                    else if (ctrl is RibbonTab tab)
                        list.Add(tab);
            }
            else
            {
                var newTabsList = new List<Control>();
                foreach (var ctrl in Tabs)
                    if (ctrl is RibbonContextualTabGroup ctx)
                        foreach (var tb in ctx.Items.OfType<RibbonTab>())
                            newTabsList.Add(tb);
                    else if (ctrl is RibbonTab tab)
                        newTabsList.Add(tab);

                ItemsSource = newTabsList;
            }
        }
    }

    private void SetChildKeyTipsVisibility(bool open)
    {
        foreach (var t in Items.OfType<RibbonTab>())
            if (t.IsVisible)
                KeyTip.GetKeyTip(t).IsOpen = open;
        if (Menu is Control menuControl)
            KeyTip.GetKeyTip(menuControl).IsOpen = open;
    }

    protected internal bool TryNavigateBackFromTabKeyTips()
    {
        if (SelectedItem is not RibbonTab tab)
            return false;

        if (!KeyTip.GetShowChildKeyTipKeys(tab))
            return false;

        KeyTip.SetShowChildKeyTipKeys(tab, false);
        KeyTip.SetShowChildKeyTipKeys(this, true);
        return true;
    }

    protected internal bool TryHandleShortcut(Key key, KeyModifiers modifiers)
    {
        if (IsOpen)
            return false;

        foreach (var control in EnumerateSelectedGroupControls())
        {
            if (!control.IsEnabled || !control.IsEffectivelyVisible)
                continue;

            switch (control)
            {
                case RibbonButton button when ShortcutMatches(button.ShortcutKeys, key, modifiers):
                    return ExecuteShortcutCommand(button.Command, button.CommandParameter, button);
                case RibbonToggleButton toggleButton when ShortcutMatches(toggleButton.ShortcutKeys, key, modifiers):
                    return ExecuteShortcutCommand(toggleButton.Command, toggleButton.CommandParameter, toggleButton);
                case RibbonSplitButton splitButton when ShortcutMatches(splitButton.ShortcutKeys, key, modifiers):
                    return ExecuteShortcutCommand(splitButton.Command, splitButton.CommandParameter, splitButton);
                case SplitButtonControl splitButtonControl when ShortcutMatches(splitButtonControl.ShortcutKeys, key, modifiers):
                    return ExecuteShortcutCommand(splitButtonControl.Command, splitButtonControl.CommandParameter, splitButtonControl);
                case RibbonDropDownButton dropDownButton when ShortcutMatches(dropDownButton.ShortcutKeys, key, modifiers):
                    dropDownButton.IsDropDownOpen = true;
                    return true;
            }
        }

        return false;
    }

    private static bool ShortcutMatches(KeyGesture? shortcut, Key key, KeyModifiers modifiers)
    {
        return shortcut != null && shortcut.Key == key && shortcut.KeyModifiers == modifiers;
    }

    private static bool ExecuteShortcutCommand(ICommand? command, object? parameter, Control source)
    {
        if (command != null)
        {
            if (!command.CanExecute(parameter))
                return false;

            command.Execute(parameter);
            return true;
        }

        source.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        return true;
    }

    private IEnumerable<Control> EnumerateSelectedGroupControls()
    {
        foreach (var group in SelectedGroups)
        {
            foreach (var control in EnumerateGroupControls(group))
                yield return control;
        }
    }

    private static IEnumerable<Control> EnumerateGroupControls(RibbonGroupBox group)
    {
        foreach (var control in group.Items.OfType<Control>())
        {
            yield return control;

            if (control is RibbonGroupContainer container)
            {
                foreach (var nested in EnumerateContainerControls(container))
                    yield return nested;
            }
        }
    }

    private static IEnumerable<Control> EnumerateContainerControls(RibbonGroupContainer container)
    {
        foreach (var child in container.Children.OfType<Control>())
        {
            yield return child;

            if (child is RibbonGroupContainer nestedContainer)
            {
                foreach (var nested in EnumerateContainerControls(nestedContainer))
                    yield return nested;
            }
        }
    }

    private sealed class QuickAccessItemsCollection : ObservableCollection<ICanAddToQuickAccess>
    {
        public QuickAccessItemsCollection()
        {
        }

        protected override void InsertItem(int index, ICanAddToQuickAccess item)
        {
            if (item == null || Contains(item))
                return;

            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, ICanAddToQuickAccess item)
        {
            if (item == null)
                return;

            var existingIndex = IndexOf(item);
            if (existingIndex >= 0 && existingIndex != index)
                return;

            base.SetItem(index, item);
        }
    }

    /*private object _selectedContent;
    private IDataTemplate _selectedContentTemplate;

    /// <summary>
    /// Gets or sets the default data template used to display the content of the selected tab.
    /// </summary>
    public IDataTemplate ContentTemplate
    {
        get => this.GetValue<IDataTemplate>(ContentTemplateProperty);
        set => this.SetValue<IDataTemplate>(ContentTemplateProperty, value);
    }
    /// <summary>Gets or sets the content of the selected tab.</summary>
    /// <value>The content of the selected tab.</value>
    public object SelectedContent
    {
        get => this._selectedContent;
        internal set => this.SetAndRaise<object>((DirectPropertyBase<object>) SelectedContentProperty, ref this._selectedContent, value);
    }

    /// <summary>
    /// Gets or sets the horizontal alignment of the content within the control.
    /// </summary>
    public HorizontalAlignment HorizontalContentAlignment
    {
        get => this.GetValue<HorizontalAlignment>(TabControl.HorizontalContentAlignmentProperty);
        set => this.SetValue<HorizontalAlignment>(TabControl.HorizontalContentAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical alignment of the content within the control.
    /// </summary>
    public VerticalAlignment VerticalContentAlignment
    {
        get => this.GetValue<VerticalAlignment>(TabControl.VerticalContentAlignmentProperty);
        set => this.SetValue<VerticalAlignment>(TabControl.VerticalContentAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets the content template for the selected tab.
    /// </summary>
    /// <value>The content template of the selected tab.</value>
    public IDataTemplate SelectedContentTemplate
    {
        get => this._selectedContentTemplate;
        internal set => this.SetAndRaise<IDataTemplate>((DirectPropertyBase<IDataTemplate>) SelectedContentTemplateProperty, ref this._selectedContentTemplate, value);
    }*/

    protected void UpdatePresenterLocation(bool intoFlyout)
    {
        if (_groupsHost == null || _flyoutPresenter == null || _mainPresenter == null)
            return;

        if (_groupsHost.Parent is ContentPresenter presenter)
            presenter.Content = null;
        else if (_groupsHost.Parent is ContentControl control)
            control.Content = null;
        else if (_groupsHost.Parent is Panel panel)
            panel.Children.Remove(_groupsHost);

        if (intoFlyout)
            _flyoutPresenter.Content = _groupsHost;
        else
            _mainPresenter.Content = _groupsHost;
    }

    #endregion Methods
}
