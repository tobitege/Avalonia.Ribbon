using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using AvaloniaUI.Ribbon.Models;

namespace AvaloniaUI.Ribbon.Tests;

public class GalleryTests
{
    [Fact]
    public void BringIntoView_ScrollsToRequestedIndex()
    {
        var gallery = new Gallery
        {
            Size = RibbonControlSize.Medium,
            ItemHeight = 20
        };

        for (var i = 0; i < 12; i++)
            gallery.Items.Add($"Item {i}");

        var presenter = new GalleryScrollContentPresenter();
        presenter.Arrange(new Rect(0, 0, 100, 100));

        var host = new ContentControl();
        host.Arrange(new Rect(0, 0, 100, 280));

        SetPrivateField(gallery, "_scrollPresenter", presenter);
        SetPrivateField(gallery, "_mainPresenter", host);

        gallery.BringIntoView(7);

        Assert.Equal(60, presenter.Offset.Y, 3);
    }

    [Fact]
    public void RangesCollection_IsAvailableAndMutable()
    {
        var gallery = new Gallery();
        var range = new GalleryRange { Header = "Theme Colors", StartIndex = 0, Count = 8 };

        gallery.Ranges.Add(range);

        Assert.Single(gallery.Ranges);
        Assert.Same(range, gallery.Ranges[0]);
    }

    [Fact]
    public void HoverTracking_RaisesItemHoverChangedEvent()
    {
        var gallery = new Gallery();
        gallery.Items.Add("Item A");

        var galleryItem = new GalleryItem
        {
            DataContext = "Item A"
        };

        GalleryItemHoverChangedEventArgs? raisedArgs = null;
        gallery.ItemHoverChanged += (_, args) => raisedArgs = args;

        var raiseMethod = typeof(Gallery).GetMethod("RaiseItemHoverChanged",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(raiseMethod);
        raiseMethod!.Invoke(gallery, new object[] { galleryItem, true });

        Assert.NotNull(raisedArgs);
        Assert.Equal(0, raisedArgs!.Index);
        Assert.Equal("Item A", raisedArgs.Item);
        Assert.True(raisedArgs.IsHovering);
    }

    private static void SetPrivateField(object target, string fieldName, object? value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        field!.SetValue(target, value);
    }
}
