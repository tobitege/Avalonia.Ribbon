# Main Library Controls Reference

This document is the implementation-oriented reference for controls in `AvaloniaUI.Ribbon`.

## Scope

- Library: `AvaloniaUI.Ribbon`
- Includes user-facing controls and key layout infrastructure controls that affect runtime behavior.

## Ribbon anatomy (mental model)

When reading this file, use the following structure as the baseline ribbon model:

```text
┌──────────────────────────────────────────────────────────────────────────────┐
│ File / App Menu | Home | Insert | View | Picture Tools: Format             │
│                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^            │
│                  Ribbon tab header (tab strip + contextual tabs)           │
├──────────────────────────────────────────────────────────────────────────────┤
│ [Clipboard Group] [Editing Group] [Layout Group] [Styles Group]            │
│  └─ buttons/toggles/splits/combos/galleries inside each RibbonGroupBox     │
│                                                                             │
│ Active tab content surface (group row, shrink/wrap behavior when narrow)   │
└──────────────────────────────────────────────────────────────────────────────┘
```

Usual parts of a ribbon UI:

- App/File entry point (often backstage-style menu).
- Ribbon tab header (the tab strip users click to switch command contexts).
- Active tab content surface with grouped commands.
- Contextual tab groups that appear only in specific contexts.
- Adaptive behavior when width is constrained (shrink, wrap, collapse, overflow).

## Microsoft ribbon comparison (practical)

Modern desktop apps still use the same core ribbon anatomy, but with product-specific differences (classic vs simplified surfaces, optional command search, and app-specific layouts).

For this library, the practical mapping is:

| Common modern ribbon pattern | `AvaloniaUI.Ribbon` status | Notes |
| --- | --- | --- |
| App menu / backstage area | Supported | `RibbonMenu` + `RibbonMenuItem` in main library. |
| Ribbon tab header | Supported | `RibbonTab` and `RibbonContextualTabGroup`. |
| Contextual tabs shown only for relevant content | Supported | Toggle `RibbonContextualTabGroup.IsVisible`. |
| Grouped commands on active tab surface | Supported | `RibbonGroupBox` + command controls. |
| Group-level adaptive layout when narrow | Supported | `GroupOverflowBehavior` + `MaxGroupRows`. |
| Simplified vs classic ribbon mode toggle | Partial | Collapse/overflow exists; no dedicated built-in simplified/classic mode API. |
| Quick Access Toolbar support | Core + Desktop | Core exposes QAT API (`QuickAccessItems`, `QuickAccessLocation`, `ShowQatOverflowButton`), enforces reference-unique default QAT items, and `Desktop` hosts/toggles add-remove via title bar or inline context menu. |

External reference points for the baseline model:

- Windows ribbon framework overview: <https://learn.microsoft.com/en-us/windows/win32/windowsribbon/windowsribbon-introduction>
- Old Windows 7 ribbon UI design guidance: <https://learn.microsoft.com/en-us/windows/win32/uxguide/cmd-ribbons>
- Microsoft 365 simplified/classic ribbon example (Outlook): <https://support.microsoft.com/en-us/office/use-the-simplified-ribbon-44bef9c3-295d-4092-b7f0-f471fa629a98>

## Shared sizing model

Most ribbon input controls use `RibbonControlHelper<T>` in `AvaloniaUI.Ribbon/Helpers/RibbonControlHelper.cs`.

Shared properties:

- `Size` (`RibbonControlSize`): default `Large`
- `MinSize` (`RibbonControlSize`): default enum default (`Small`)
- `MaxSize` (`RibbonControlSize`): default `Large`

Behavior:

- `Size` is coerced into `[MinSize, MaxSize]`.
- Changing `MinSize`/`MaxSize` can force `Size` to adjust immediately.

## Control index

| Control | Class | Primary source | Primary Fluent style |
| --- | --- | --- | --- |
| Ribbon | `Ribbon` | `AvaloniaUI.Ribbon/Ribbon.cs` | `AvaloniaUI.Ribbon/Styles/Fluent/Controls/Ribbon.axaml` |
| Ribbon tab | `RibbonTab` | `AvaloniaUI.Ribbon/RibbonTab.cs` | `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonTab.axaml` |
| Contextual tab group | `RibbonContextualTabGroup` | `AvaloniaUI.Ribbon/RibbonContextualTabGroup.cs` | `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonContextualTabGroup.axaml` |
| Group box | `RibbonGroupBox` | `AvaloniaUI.Ribbon/RibbonGroupBox.cs` | `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonGroupBox.axaml` |
| Group triple | `RibbonGroupTriple` | `AvaloniaUI.Ribbon/RibbonGroupTriple.cs` | `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonGroupContainers.axaml` |
| Group lines | `RibbonGroupLines` | `AvaloniaUI.Ribbon/RibbonGroupLines.cs` | `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonGroupContainers.axaml` |
| Group cluster | `RibbonGroupCluster` | `AvaloniaUI.Ribbon/RibbonGroupCluster.cs` | `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonGroupContainers.axaml` |
| Ribbon button | `RibbonButton` | `AvaloniaUI.Ribbon/RibbonButton.cs` | `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonButton.axaml` |
| Ribbon toggle button | `RibbonToggleButton` | `AvaloniaUI.Ribbon/RibbonToggleButton.cs` | `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonToggleButton.axaml` |
| Ribbon drop-down button | `RibbonDropDownButton` | `AvaloniaUI.Ribbon/RibbonDropDownButton.cs` | `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonDropDownButton.axaml` |
| Ribbon split button | `RibbonSplitButton` | `AvaloniaUI.Ribbon/RibbonSplitButton.cs` | `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonSplitButton.axaml` |
| Split button control | `SplitButtonControl` | `AvaloniaUI.Ribbon/SplitButtonControl.cs` | `AvaloniaUI.Ribbon/Styles/Fluent/Controls/SplitButtonControl.axaml` |
| Ribbon combo box | `RibbonComboBox` | `AvaloniaUI.Ribbon/RibbonComboBox.cs` | `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonComboBox.axaml` |
| Ribbon text box | `RibbonTextBox` | `AvaloniaUI.Ribbon/RibbonTextBox.cs` | `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonTextBox.axaml` |
| Ribbon date picker | `RibbonDatePicker` | `AvaloniaUI.Ribbon/RibbonDatePicker.cs` | `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonDatePicker.axaml` |
| Ribbon numeric up-down | `RibbonNumericUpDown` | `AvaloniaUI.Ribbon/RibbonNumericUpDown.cs` | `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonNumericUpDown.axaml` |
| Ribbon check box | `RibbonCheckBox` | `AvaloniaUI.Ribbon/RibbonCheckBox.cs` | `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonCheckBox.axaml` |
| Ribbon radio button | `RibbonRadioButton` | `AvaloniaUI.Ribbon/RibbonRadioButton.cs` | `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonRadioButton.axaml` |
| Ribbon label | `RibbonLabel` | `AvaloniaUI.Ribbon/RibbonLabel.cs` | `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonLabel.axaml` |
| Ribbon separator | `RibbonSeparator` | `AvaloniaUI.Ribbon/RibbonSeparator.cs` | `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonSeparator.axaml` |
| Ribbon gallery | `Gallery` | `AvaloniaUI.Ribbon/Gallery.cs` | `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonGallery.axaml` |
| Gallery item | `GalleryItem` | `AvaloniaUI.Ribbon/GalleryItem.cs` | `AvaloniaUI.Ribbon/Styles/Fluent/Controls/GalleryItem.axaml` |
| Ribbon menu (backstage) | `RibbonMenu` | `AvaloniaUI.Ribbon/RibbonMenu.cs` | `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonMenu.axaml` |
| Ribbon menu item | `RibbonMenuItem` | `AvaloniaUI.Ribbon/RibbonMenuItem.cs` | `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonMenu.axaml` |
| Ribbon drop-down item | `RibbonDropDownItem` | `AvaloniaUI.Ribbon/RibbonDropDownItem.cs` | `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonDropDownItem.axaml` |
| Drop-down items presenter | `RibbonDropDownItemsPresenter` | `AvaloniaUI.Ribbon/RibbonDropDownItemsPresenter.cs` | `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonDropDownItemsPresenter.axaml` |
| Drop-down separator | `RibbonDropDownSeparator` | `AvaloniaUI.Ribbon/RibbonDropDownSeperator.cs` | `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonDropDownSeperator.axaml` |
| Groups stack panel (layout engine) | `RibbonGroupsStackPanel` | `AvaloniaUI.Ribbon/RibbonGroupsStackPanel.cs` | `AvaloniaUI.Ribbon/Styles/Fluent/Controls/Ribbon.axaml` |
| Group wrap panel (legacy helper) | `RibbonGroupWrapPanel` | `AvaloniaUI.Ribbon/RibbonGroupWrapPanel.cs` | `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonGroupBox.axaml` |
| Group container base | `RibbonGroupContainer` | `AvaloniaUI.Ribbon/RibbonGroupContainer.cs` | `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonGroupContainers.axaml` |

---

## Ribbon

Source: `AvaloniaUI.Ribbon/Ribbon.cs`

Purpose:

- Root ribbon control hosting tabs, groups, and backstage/menu integration.
- Handles collapse behavior, key-tip activation, and group overflow options.

Key properties:

| Property | Type | Default |
| --- | --- | --- |
| `Orientation` | `Orientation` | `Horizontal` |
| `GroupOverflowBehavior` | `RibbonGroupOverflowBehavior` | `ShrinkOnly` |
| `MaxGroupRows` | `int` | `1` (coerced to `>= 1`) |
| `IsCollapsed` | `bool` | `false` |
| `IsCollapsedPopupOpen` | `bool` | `false` |
| `Menu` | `IRibbonMenu` | `null` |
| `ContextualTabGroups` | `IList<RibbonContextualTabGroup>` | initialized in ctor |
| `QuickAccessItems` | `ObservableCollection<ICanAddToQuickAccess>` | empty (reference-unique default collection) |
| `QuickAccessLocation` | `RibbonQatLocation` | `RibbonQatLocation.Above` |
| `ShowQatOverflowButton` | `bool` | `true` |

Template parts:

- `PART_CollapsedContentPopup`
- `PART_SelectedGroupsHost`
- `PART_GroupsPresenterHolder`
- `PART_PopupGroupsPresenterHolder`
- `PART_ItemsPresenter`
- `PART_PinLastHoveredControlToQuickAccess`
- `PART_ContentAreaContextMenu`
- `PART_CollapseRibbon`

Customization:

- Visual + template behavior: `AvaloniaUI.Ribbon/Styles/Fluent/Controls/Ribbon.axaml`
- Overflow behavior is implemented by `RibbonGroupsStackPanel`.

## RibbonTab

Source: `AvaloniaUI.Ribbon/RibbonTab.cs`

Purpose:

- Represents one tab and owns its `Groups` collection.
- Participates in key-tip command routing.

Key properties:

| Property | Type | Default |
| --- | --- | --- |
| `Groups` | `ObservableCollection<RibbonGroupBox>` | new collection |
| `IsContextual` | `bool` | `false` |

Customization:

- `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonTab.axaml`

## RibbonContextualTabGroup

Source: `AvaloniaUI.Ribbon/RibbonContextualTabGroup.cs`

Purpose:

- Represents a contextual tab band (for example Picture Tools / Table Tools style experiences).
- Use it when tabs should appear only for a specific app context, such as selecting an image, chart, or table.
- Keeps child `RibbonTab.IsContextual` in sync and safely moves selection away if the contextual group is hidden.

Where it appears in UI:

- It lives inside `Ribbon.Tabs`, alongside regular `RibbonTab` entries.
- Its child tabs render in the ribbon tab header area as contextual tabs (typically grouped/colored by style).

Typical usage pattern:

```xaml
<Ribbon>
    <Ribbon.Tabs>
        <RibbonTab Header="Home" />

        <RibbonContextualTabGroup
            Header="Picture Tools"
            IsVisible="{Binding IsImageSelected}">
            <RibbonTab Header="Format">
                <RibbonTab.Groups>
                    <RibbonGroupBox Header="Adjust" />
                </RibbonTab.Groups>
            </RibbonTab>
        </RibbonContextualTabGroup>
    </Ribbon.Tabs>
</Ribbon>
```

Behavior notes:

- Toggle visibility with `IsVisible` on `RibbonContextualTabGroup`.
- When `IsVisible` becomes `false` while one of its tabs is selected, the control attempts to switch to another visible tab automatically.
- `ContextColor` is the dedicated contextual tint API; `Background` stays synchronized for backward compatibility.

Customization:

- `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonContextualTabGroup.axaml`

## RibbonGroupBox

Source: `AvaloniaUI.Ribbon/RibbonGroupBox.cs`

Purpose:

- Container for commands/groups under a tab.
- Propagates display mode to child containers/controls.
- Fluent default template uses `RibbonGroupWrapPanel` for internal command layout.

Key properties:

| Property | Type | Default |
| --- | --- | --- |
| `DisplayMode` | `GroupDisplayMode` | `Small` |
| `DialogLauncherCommand` | `ICommand` | `null` |
| `DialogLauncherCommandParameter` | `object` | `null` |
| `Command` / `CommandParameter` | alias to dialog launcher properties | `null` |
| `AllowCollapsedPopup` | `bool` | `true` |
| `IsCollapsedToPopup` | `bool` (read-only/direct) | `false` |

Events:

- `Rearranged`
- `Remeasured`

Customization:

- `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonGroupBox.axaml`

## RibbonGroupTriple / RibbonGroupLines / RibbonGroupCluster

Sources:

- `AvaloniaUI.Ribbon/RibbonGroupTriple.cs`
- `AvaloniaUI.Ribbon/RibbonGroupLines.cs`
- `AvaloniaUI.Ribbon/RibbonGroupCluster.cs`

Purpose:

- Canonical ribbon sub-layout primitives for stacks, line-banks, and compact clusters.

Detailed guide:

- `docs/group-containers.md`

Shared style location:

- `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonGroupContainers.axaml`

## RibbonButton

Source: `AvaloniaUI.Ribbon/RibbonButton.cs`

Purpose:

- Base ribbon push-button with ribbon sizing and quick-access metadata.

Key properties:

| Property | Type | Default |
| --- | --- | --- |
| `CanAddToQuickAccess` | `bool` | `true` |
| `Icon` | `object` | `null` |
| `LargeIcon` | `object` | `null` |
| `QuickAccessIcon` | `object` | `null` |
| `QuickAccessTemplate` | `IControlTemplate` | `null` |
| `ShortcutKeys` | `KeyGesture` | `null` |
| `Size` | `RibbonControlSize` | `Large` |
| `MinSize` | `RibbonControlSize` | `Small` |
| `MaxSize` | `RibbonControlSize` | `Large` |

Notes:

- `Focusable` default is overridden to `false`.

Customization:

- `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonButton.axaml`

## RibbonToggleButton

Source: `AvaloniaUI.Ribbon/RibbonToggleButton.cs`

Purpose:

- Ribbon toggle variant with the same icon + size model as `RibbonButton`.

Key properties:

- `Icon`, `LargeIcon`, `QuickAccessIcon`, `CanAddToQuickAccess`, `QuickAccessTemplate`
- `ShortcutKeys`
- `Size`, `MinSize`, `MaxSize`

Notes:

- `Focusable` default is overridden to `false`.

Customization:

- `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonToggleButton.axaml`

## RibbonDropDownButton

Source: `AvaloniaUI.Ribbon/RibbonDropDownButton.cs`

Purpose:

- Ribbon button with flyout/drop-down content and item collection.

Template parts:

- `PART_PrimaryButton`

Key properties:

| Property | Type | Default |
| --- | --- | --- |
| `Content` | `object?` | inherited |
| `IsDropDownOpen` | `bool` | `false` |
| `Icon` / `LargeIcon` | `object` | `null` |
| `ShortcutKeys` | `KeyGesture` | `null` |
| `QuickAccessIcon` / `QuickAccessTemplate` | ribbon quick-access model | inherited |
| `Size` / `MinSize` / `MaxSize` | ribbon sizing model | helper defaults |

Behavior notes:

- Synchronizes `IsDropDownOpen` with `Flyout.Opened`/`Flyout.Closed`.

Customization:

- `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonDropDownButton.axaml`

## RibbonSplitButton

Source: `AvaloniaUI.Ribbon/RibbonSplitButton.cs`

Purpose:

- Command-capable split-button built on `RibbonDropDownButton`.

Extra properties:

- `Command`
- `CommandParameter`
- `ShortcutKeys`

Customization:

- `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonSplitButton.axaml`

## SplitButtonControl

Source: `AvaloniaUI.Ribbon/SplitButtonControl.cs`

Purpose:

- Wrapper over Avalonia `SplitButton` implementing ribbon size/icon/quick-access contracts.

Key properties:

- `Icon`, `LargeIcon`, `IsDropDownOpen`
- `ShortcutKeys`
- `CanAddToQuickAccess`, `QuickAccessIcon`, `QuickAccessTemplate`
- `Size`, `MinSize`, `MaxSize`

Behavior note:

- On `Size=Large`, it explicitly tries to apply `LargeSplitButton` theme from resources.

Customization:

- `AvaloniaUI.Ribbon/Styles/Fluent/Controls/SplitButtonControl.axaml`

## RibbonComboBox

Source: `AvaloniaUI.Ribbon/RibbonComboBox.cs`

Purpose:

- ComboBox adapted to ribbon sizing and icon contracts.

Key properties:

- `Content`, `Icon`, `LargeIcon`
- `Size`, `MinSize`, `MaxSize`

Notes:

- `Focusable` default is overridden to `false`.

Customization:

- `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonComboBox.axaml`

## RibbonTextBox / RibbonDatePicker / RibbonNumericUpDown

Sources:

- `AvaloniaUI.Ribbon/RibbonTextBox.cs`
- `AvaloniaUI.Ribbon/RibbonDatePicker.cs`
- `AvaloniaUI.Ribbon/RibbonNumericUpDown.cs`

Purpose:

- Ribbon-native text/date/numeric input controls that participate in ribbon `Size`/`MinSize`/`MaxSize`.

Customization:

- `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonTextBox.axaml`
- `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonDatePicker.axaml`
- `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonNumericUpDown.axaml`

## RibbonCheckBox / RibbonRadioButton / RibbonLabel / RibbonSeparator

Sources:

- `AvaloniaUI.Ribbon/RibbonCheckBox.cs`
- `AvaloniaUI.Ribbon/RibbonRadioButton.cs`
- `AvaloniaUI.Ribbon/RibbonLabel.cs`
- `AvaloniaUI.Ribbon/RibbonSeparator.cs`

Purpose:

- Ribbon-native option/annotation/separator controls with ribbon sizing contracts.

Customization:

- `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonCheckBox.axaml`
- `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonRadioButton.axaml`
- `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonLabel.axaml`
- `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonSeparator.axaml`

## Gallery (RibbonGallery)

Source: `AvaloniaUI.Ribbon/Gallery.cs`

Purpose:

- Scrollable ribbon gallery with optional flyout expansion.

Template parts:

- `PART_ItemsPresenter`
- `PART_ItemsPresenterHolder`
- `PART_UpButton`
- `PART_DownButton`
- `PART_ScrollContentPresenter`
- `PART_FlyoutItemsPresenterHolder`
- `PART_FlyoutRoot`

Key properties:

| Property | Type | Default |
| --- | --- | --- |
| `IsDropDownOpen` | `bool` | inherited |
| `ItemHeight` | `double` | `0` unless set |
| `Ranges` | `ObservableCollection<GalleryRange>` | empty |
| `Size` / `MinSize` / `MaxSize` | ribbon sizing model | helper defaults |

API additions:

- `BringIntoView(int index)`
- `ItemHoverChanged` event (`GalleryItemHoverChangedEventArgs`)

Behavior notes:

- Moves shared `ItemsPresenter` between inline and flyout hosts.
- Up/down buttons move presenter offset by `ItemHeight`.
- `BringIntoView(int index)` scrolls to the target row/index using current gallery size column rules.

Customization:

- `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonGallery.axaml`

## GalleryItem

Source: `AvaloniaUI.Ribbon/GalleryItem.cs`

Purpose:

- Item container for gallery entries.

Key properties:

- `Icon`
- `LargeIcon`

Customization:

- `AvaloniaUI.Ribbon/Styles/Fluent/Controls/GalleryItem.axaml`

## RibbonMenu (backstage)

Source: `AvaloniaUI.Ribbon/RibbonMenu.cs`

Purpose:

- Backstage-style menu with grouped top-docked and bottom-docked menu items.

Template parts:

- `MenuPopup` (required)

Key properties:

| Property | Type | Default |
| --- | --- | --- |
| `IsMenuOpen` | `bool` | `false` |
| `Content` | `object` | inherited |
| `SelectedItemContent` | `object` | `null` |
| `SelectedSubItems` | `object` | `null` |
| `TopDockedGroupedItems` | grouped collection | computed |
| `BottomDockedGroupedItems` | grouped collection | computed |
| `RecentDocuments` | `ObservableCollection<RibbonRecentDocument>` | empty |
| `RecentDocumentClickCommand` | `ICommand` | initialized in ctor |

Behavior notes:

- Regroups items by `RibbonMenuItem.Group`.
- Updates `IsLastItem` flags per group.
- Recent-doc entries execute per-item command and raise `RecentDocumentInvoked`.

Customization:

- `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonMenu.axaml`

## RibbonRecentDocument

Source: `AvaloniaUI.Ribbon/Models/RibbonRecentDocument.cs`

Purpose:

- Data model for the dedicated recent-documents section in `RibbonMenu`.

Key properties:

- `Title`
- `Path`
- `Icon`
- `Command` / `CommandParameter`

## RibbonMenuItem

Source: `AvaloniaUI.Ribbon/RibbonMenuItem.cs`

Purpose:

- Menu item model used by `RibbonMenu` grouping/docking logic.

Template parts:

- `PART_ContentButton`

Key properties:

| Property | Type | Default |
| --- | --- | --- |
| `Group` | `string` | `null` |
| `IsTopDocked` | `bool` | `true` |
| `IsBottomDocked` | `bool` | `false` |
| `IsLastItem` | `bool` | `false` |
| `IsSubmenuOpen` | `bool` | `false` |
| `IsSelected` | `bool` | `false` |
| `Command` / `CommandParameter` | command model | inherited |

Events:

- `Click`

Customization:

- `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonMenu.axaml`

## RibbonDropDownItem

Source: `AvaloniaUI.Ribbon/RibbonDropDownItem.cs`

Purpose:

- Styled menu item used inside ribbon drop-down flyouts.

Customization:

- `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonDropDownItem.axaml`

## RibbonDropDownItemsPresenter

Source: `AvaloniaUI.Ribbon/RibbonDropDownItemsPresenter.cs`

Purpose:

- Presenter control for grouped/headered drop-down content.

Customization:

- `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonDropDownItemsPresenter.axaml`

## RibbonDropDownSeparator

Source: `AvaloniaUI.Ribbon/RibbonDropDownSeperator.cs`

Purpose:

- Separator control for ribbon drop-down menus.

Customization:

- `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonDropDownSeperator.axaml`

## RibbonGroupsStackPanel (layout infrastructure)

Source: `AvaloniaUI.Ribbon/RibbonGroupsStackPanel.cs`

Purpose:

- Core layout algorithm for ribbon groups.
- Implements `ShrinkOnly` and `WrapThenShrink` behavior, including re-expansion when space returns.

Key properties:

| Property | Type | Default |
| --- | --- | --- |
| `Orientation` | `Orientation` | inherited |
| `GroupOverflowBehavior` | `RibbonGroupOverflowBehavior` | inherited from `Ribbon` |
| `MaxGroupRows` | `int` | inherited from `Ribbon` |

Notes:

- This is primarily a framework/internal layout surface.

## RibbonGroupWrapPanel

Source: `AvaloniaUI.Ribbon/RibbonGroupWrapPanel.cs`

Purpose:

- Alternative wrap-panel based group arranger that applies `DisplayMode` to child controls.

Key properties:

- `DisplayMode` (`GroupDisplayMode`, inherited owner pattern)
- `LargeLineCount` (`int`, default `3`)
- `SmallLineCount` (`int`, default `3`)

Notes:

- The modern overflow behavior is centered on `RibbonGroupsStackPanel`.
- In horizontal `WrapThenShrink`, default template binding caps `SmallLineCount` with ribbon `MaxGroupRows`.

## RibbonGroupContainer (base class)

Source: `AvaloniaUI.Ribbon/RibbonGroupContainer.cs`

Purpose:

- Abstract base for group container controls.

Key properties:

| Property | Type | Default |
| --- | --- | --- |
| `DisplayMode` | `GroupDisplayMode` | inherited |
| `MinimumSize` | `RibbonControlSize` | `Small` |
| `MaximumSize` | `RibbonControlSize` | `Large` |
| `ItemSpacing` | `double` | `2` |
| `CurrentSize` | `RibbonControlSize` | computed |

Behavior:

- Applies display mode to child containers recursively.
- Clamps child `IRibbonControl.Size` to child `MinSize`/`MaxSize`.

---

## Practical extension rules

- For visual changes, prefer editing control-specific files in `AvaloniaUI.Ribbon/Styles/Fluent/Controls/`.
- For behavior changes, start in the corresponding control `.cs` file listed above.
- For any change affecting ribbon group layout/overflow:
- inspect `Ribbon.cs` + `RibbonGroupsStackPanel.cs` together.
- For any change affecting grouped container composition:
- inspect `RibbonGroupContainer.cs` plus specific group container implementation.
