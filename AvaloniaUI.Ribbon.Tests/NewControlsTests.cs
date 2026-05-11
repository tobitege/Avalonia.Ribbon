using AvaloniaUI.Ribbon.Contracts;
using AvaloniaUI.Ribbon.Models;

namespace AvaloniaUI.Ribbon.Tests;

public class NewControlsTests
{
    public static IEnumerable<object[]> ControlFactories()
    {
        yield return new object[] { typeof(RibbonTextBox) };
        yield return new object[] { typeof(RibbonDatePicker) };
        yield return new object[] { typeof(RibbonNumericUpDown) };
        yield return new object[] { typeof(RibbonCheckBox) };
        yield return new object[] { typeof(RibbonRadioButton) };
        yield return new object[] { typeof(RibbonLabel) };
        yield return new object[] { typeof(RibbonSeparator) };
    }

    [Theory]
    [MemberData(nameof(ControlFactories))]
    public void NewControls_SizeContracts_CoerceWithinMinMax(Type controlType)
    {
        var control = CreateControl(controlType);

        control.MinSize = RibbonControlSize.Medium;
        control.MaxSize = RibbonControlSize.Medium;

        control.Size = RibbonControlSize.Small;
        Assert.Equal(RibbonControlSize.Medium, control.Size);

        control.Size = RibbonControlSize.Large;
        Assert.Equal(RibbonControlSize.Medium, control.Size);
    }

    [Theory]
    [MemberData(nameof(ControlFactories))]
    public void NewControls_SupportLargeMediumSmallRenderStates(Type controlType)
    {
        var control = CreateControl(controlType);

        control.MinSize = RibbonControlSize.Small;
        control.MaxSize = RibbonControlSize.Large;

        control.Size = RibbonControlSize.Large;
        Assert.Equal(RibbonControlSize.Large, control.Size);

        control.Size = RibbonControlSize.Medium;
        Assert.Equal(RibbonControlSize.Medium, control.Size);

        control.Size = RibbonControlSize.Small;
        Assert.Equal(RibbonControlSize.Small, control.Size);
    }

    private static IRibbonControl CreateControl(Type controlType)
    {
        return Assert.IsAssignableFrom<IRibbonControl>(Activator.CreateInstance(controlType));
    }
}
