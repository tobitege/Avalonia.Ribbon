using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using AvaloniaUI.Ribbon.Desktop;

namespace AvaloniaUI.Ribbon.Demo.Views;

public partial class DocumentView : UserControl
{
    public DocumentView()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        var isRibbonWindow = TopLevel.GetTopLevel(this) is RibbonWindow;
        DesktopChromeOptionsPanel.IsVisible = isRibbonWindow;
        ShowTitleBarIconCheckBox.IsVisible = isRibbonWindow;
    }

    private void OnDecorationModeSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var comboBox = sender as ComboBox;
        if (comboBox == null || !comboBox.IsDropDownOpen ||
            !(comboBox.SelectedValue is WindowDecorations))
            return;

        var windowDecorations = (WindowDecorations)comboBox.SelectedValue;
        var window = TopLevel.GetTopLevel(this) as RibbonWindow;
        comboBox.IsDropDownOpen = false;

        if (window == null)
            return;

        Dispatcher.UIThread.Post(() =>
        {
            window.WindowDecorations = windowDecorations;
        }, DispatcherPriority.Background);
    }
}
