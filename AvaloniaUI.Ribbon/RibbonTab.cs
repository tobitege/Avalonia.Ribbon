using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaUI.Ribbon.Contracts;
using AvaloniaUI.Ribbon.Helpers;

namespace AvaloniaUI.Ribbon;

public class RibbonTab : TabItem, IKeyTipHandler
{
    public ObservableCollection<RibbonGroupBox> Groups
    {
        get => _groups;
        set => SetAndRaise(GroupsProperty, ref _groups, value);
    }

    public bool IsContextual
    {
        get => GetValue(IsContextualProperty);
        set => SetValue(IsContextualProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(RibbonTab);
    /*[Content]*/

    public void ActivateKeyTips(IRibbon ribbon, IKeyTipHandler prev)
    {
        _ribbon = ribbon;
        _prev = prev;
        foreach (var g in Groups)
            Debug.WriteLine("GROUP KEYS: " + KeyTip.GetKeyTipKeys(g));

        Focus();
        KeyTip.SetShowChildKeyTipKeys(this, true);
        KeyDown += RibbonTab_KeyDown;
    }

    public bool HandleKeyTipKeyPress(Key key)
    {
        var retVal = false;
        foreach (var g in Groups)
        {
            foreach (var c in g.Items.OfType<Control>())
                if (KeyTip.HasKeyTipKey(c, key))
                {
                    if (c is IKeyTipHandler hdlr)
                    {
                        if (_ribbon != null)
                            hdlr.ActivateKeyTips(_ribbon, this);
                        Debug.WriteLine("Group handled " + key + " for IKeyTipHandler");
                    }
                    else
                    {
                        if (c is IRibbonCommand btn && btn.Command != null)
                            btn.Command.Execute(btn.CommandParameter);
                        else
                            c.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        _ribbon?.Close();
                        retVal = true;
                    }

                    break;
                }

            if (retVal)
                break;
        }

        return retVal;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        var inputRoot = e.Root as IInputRoot;
        if (inputRoot != null && inputRoot is WindowBase wnd)
            wnd.Deactivated += InputRoot_Deactivated;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        var inputRoot = e.Root as IInputRoot;
        if (inputRoot != null && inputRoot is WindowBase wnd)
            wnd.Deactivated -= InputRoot_Deactivated;
    }

    private void InputRoot_Deactivated(object sender, EventArgs e)
    {
        KeyTip.SetShowChildKeyTipKeys(this, false);
        RibbonControlExtensions.GetParentRibbon(this)?.Close();
    }

    private void RibbonTab_KeyDown(object sender, KeyEventArgs e)
    {
        e.Handled = HandleKeyTipKeyPress(e.Key);
        if (e.Handled)
        {
            if (_ribbon != null)
                _ribbon.IsCollapsedPopupOpen = false;
        }

        KeyTip.SetShowChildKeyTipKeys(this, false);
        KeyDown -= RibbonTab_KeyDown;
    }

    public void Close()
    {
        KeyTip.SetShowChildKeyTipKeys(this, false);
    }

    #region Fields

    public static readonly DirectProperty<RibbonTab, ObservableCollection<RibbonGroupBox>> GroupsProperty =
        AvaloniaProperty.RegisterDirect<RibbonTab, ObservableCollection<RibbonGroupBox>>(nameof(Groups), o => o.Groups,
            (o, v) => o.Groups = v);

    public static readonly StyledProperty<bool> IsContextualProperty =
        AvaloniaProperty.Register<RibbonTab, bool>(nameof(IsContextual));

    private ObservableCollection<RibbonGroupBox> _groups = new();
    private IKeyTipHandler? _prev;
    private IRibbon? _ribbon;

    #endregion Fields

    #region Constructors

    static RibbonTab()
    {
        KeyTip.ShowChildKeyTipKeysProperty.Changed.AddClassHandler<RibbonTab>((sender, args) =>
        {
            if (args.NewValue is bool show && show)
                foreach (var g in sender.Groups)
                {
                    if (g.Command != null && KeyTip.HasKeyTipKeys(g))
                        KeyTip.GetKeyTip(g).IsOpen = true;

                    foreach (var c in g.Items.OfType<Control>())
                        if (KeyTip.HasKeyTipKeys(c))
                            KeyTip.GetKeyTip(c).IsOpen = true;
                }
            else
                foreach (var g in sender.Groups)
                {
                    KeyTip.GetKeyTip(g).IsOpen = false;

                    foreach (var c in g.Items.OfType<Control>())
                        KeyTip.GetKeyTip(c).IsOpen = false;
                }
        });
    }

    public RibbonTab()
    {
        LostFocus += (sneder, args) => KeyTip.SetShowChildKeyTipKeys(this, false);
    }

    #endregion Constructors
}