using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaUI.Ribbon.Desktop;

namespace AvaloniaUI.Ribbon.Demo.Flowery.Views;

public partial class DocumentView : UserControl
{
    public DocumentView()
    {
        InitializeComponent();
        LoadSeparatorResources();
    }

    private void LoadSeparatorResources()
    {
        if (Application.Current?.Resources.TryGetResource("RibbonGroupBoxSeparatorThickness", null, out var thicknessResource) == true &&
            thicknessResource is Thickness thickness)
            SeparatorThicknessNumericUpDown.Value = (decimal)thickness.Left;

        if (Application.Current?.Resources.TryGetResource("RibbonGroupBoxSeparatorBrush", null, out var brushResource) == true &&
            brushResource is ISolidColorBrush brush)
            SeparatorColorPicker.Color = brush.Color;
    }

    private void OnSeparatorThicknessChanged(object sender, NumericUpDownValueChangedEventArgs e)
    {
        if (e.NewValue.HasValue && Application.Current != null)
            Application.Current.Resources["RibbonGroupBoxSeparatorThickness"] = new Thickness((double)e.NewValue.Value);
    }

    private void OnSeparatorColorChanged(object sender, ColorChangedEventArgs e)
    {
        if (Application.Current != null)
            Application.Current.Resources["RibbonGroupBoxSeparatorBrush"] = new SolidColorBrush(e.NewColor);
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