using AvaloniaUI.Ribbon.Contracts;
using AvaloniaUI.Ribbon.Models;

namespace AvaloniaUI.Ribbon.Tests;

public class NewControlsTests
{
    public static IEnumerable<object[]> ControlFactories()
    {
        yield return new object[] { new RibbonTextBox() };
        yield return new object[] { new RibbonDatePicker() };
        yield return new object[] { new RibbonNumericUpDown() };
        yield return new object[] { new RibbonCheckBox() };
        yield return new object[] { new RibbonRadioButton() };
        yield return new object[] { new RibbonLabel() };
        yield return new object[] { new RibbonSeparator() };
    }

    [Theory]
    [MemberData(nameof(ControlFactories))]
    public void NewControls_SizeContracts_CoerceWithinMinMax(IRibbonControl control)
    {
        control.MinSize = RibbonControlSize.Medium;
        control.MaxSize = RibbonControlSize.Medium;

        control.Size = RibbonControlSize.Small;
        Assert.Equal(RibbonControlSize.Medium, control.Size);

        control.Size = RibbonControlSize.Large;
        Assert.Equal(RibbonControlSize.Medium, control.Size);
    }

    [Theory]
    [MemberData(nameof(ControlFactories))]
    public void NewControls_SupportLargeMediumSmallRenderStates(IRibbonControl control)
    {
        control.MinSize = RibbonControlSize.Small;
        control.MaxSize = RibbonControlSize.Large;

        control.Size = RibbonControlSize.Large;
        Assert.Equal(RibbonControlSize.Large, control.Size);

        control.Size = RibbonControlSize.Medium;
        Assert.Equal(RibbonControlSize.Medium, control.Size);

        control.Size = RibbonControlSize.Small;
        Assert.Equal(RibbonControlSize.Small, control.Size);
    }
}
