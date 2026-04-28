---
name: 'XAML Standards'
description: 'Coding conventions for WPF XAML files'
applyTo: '**/*.xaml'
---

# XAML Coding Conventions

## General

- Framework: **WPF (.NET 8)**
- Pattern: **MVVM** — no code-behind logic; all behavior via `{Binding}` to ViewModel
- Encoding: **UTF-8**, line endings: **CRLF**

## Localization & Static Strings

- ❌ **Never hardcode user-facing text** directly in XAML (labels, button content, window titles, column headers, etc.)
- All static strings **must** be stored in resource files located at `WpfAppCleanArchitecture/Resources/`:
  - `Strings.resx` — English (default, neutral culture)
  - `Strings.es.resx` — Spanish (`es` culture)
- The default `Strings.resx` must use `PublicResXFileCodeGenerator` to generate `Strings.Designer.cs`, enabling `{x:Static}` access
- Always declare the `res` namespace in every XAML file that uses strings:
  ```xml
  xmlns:res="clr-namespace:WpfAppCleanArchitecture.Resources"
  ```
- Reference strings via `{x:Static res:Strings.KeyName}`:
  ```xml
  <Button Content="{x:Static res:Strings.ButtonSave}" />
  <TextBlock Text="{x:Static res:Strings.LabelCustomerName}" />
  <DataGridTextColumn Header="{x:Static res:Strings.ColumnName}" ... />
  Title="{x:Static res:Strings.WindowTitleMain}"
  ```
- **Naming conventions** for resource keys (PascalCase, prefixed by category):
  | Prefix | Category | Example |
  |---|---|---|
  | `WindowTitle` | Window/dialog titles | `WindowTitleMain` |
  | `Button` | Button content | `ButtonSave`, `ButtonCancel` |
  | `Label` | Form labels / TextBlocks | `LabelCustomerName` |
  | `Column` | DataGrid column headers | `ColumnId`, `ColumnLastName` |
  | `Message` | MessageBox / status text | `MessageConfirmDelete` |
- Both `Strings.resx` and `Strings.es.resx` must always contain the **same set of keys**
- When adding a new key, add it to **both** files simultaneously

## File & Namespace Organization

- One view per file; file name matches the class name (e.g., `MainWindow.xaml`, `CustomerDialog.xaml`)
- Always declare `x:Class` matching the code-behind class
- Standard namespace declarations in this order:
  ```xml
  xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
  xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
  xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
  ```
- Add custom namespaces after standard ones, with descriptive prefixes:
  ```xml
  xmlns:converters="clr-namespace:WpfAppCleanArchitecture.Converters"
  xmlns:vm="clr-namespace:WpfAppCleanArchitecture.ViewModels"
  ```
- Always include `mc:Ignorable="d"` when using design-time namespaces

## Layout & Structure

- Use `DockPanel` as root layout for main windows with toolbars/content areas
- Use `Grid` for complex multi-column/row layouts; define `ColumnDefinitions` and `RowDefinitions` explicitly
- Use `StackPanel` for simple linear arrangements (toolbars, button rows, form fields)
- Use `WindowStartupLocation="CenterScreen"` for main windows
- Use `WindowStartupLocation="CenterOwner"` for dialogs
- Use `ResizeMode="NoResize"` for fixed-size dialogs
- Apply consistent `Margin` spacing: `10` for outer panels, `5` for inner controls, `0,0,10,0` between sibling buttons

## Naming Conventions

- Named elements: `x:Name` in PascalCase, descriptive (e.g., `CustomerDataGrid`, `SaveButton`)
- Resource keys: PascalCase (e.g., `NullToBoolConverter`, `PrimaryButtonStyle`)
- Design-time data context: `d:DataContext="{d:DesignInstance Type=vm:MainViewModel, IsDesignTimeCreatable=False}"`

## Data Binding

- Always use `{Binding PropertyName}` — never set values directly in XAML that belong to the ViewModel
- Use `Mode=TwoWay` explicitly only when the default is not `TwoWay` (e.g., `SelectedItem`)
- Use `UpdateSourceTrigger=PropertyChanged` for `TextBox` bindings that need immediate validation feedback
- Use converters for any non-trivial binding logic; never use code-behind for view logic
- Declare converters as static resources in `<Window.Resources>` or `<Application.Resources>`

## Commands

- Bind all button actions to ViewModel commands via `Command="{Binding XxxCommand}"`
- Use `IsEnabled="{Binding SomeProperty, Converter={StaticResource ...}}"` for conditional enabling — never manipulate `IsEnabled` in code-behind
- Use `IsDefault="True"` on the primary action button (e.g., Save) in dialogs
- Use `IsCancel="True"` on the cancel button in dialogs

## DataGrid

- Always set `AutoGenerateColumns="False"` and define columns explicitly
- Always set `CanUserAddRows="False"` unless row addition is a feature
- Use `IsReadOnly="True"` on ID/key columns
- Use `Width="*"` on the main content column; `Width="Auto"` on narrow columns (ID)
- Bind `SelectedItem` with `Mode=TwoWay` to the ViewModel's selected property

## Resources & Styles

- Define window-scoped resources in `<Window.Resources>`
- Define application-wide resources in `App.xaml` under `<Application.Resources>`
- Prefer named styles with `x:Key`; avoid implicit styles unless intentionally applying to all elements of a type
- Reference the [C# Standards](./csharp.instructions.md) for converter class naming (`NullToBoolConverter`, etc.)

## Dialogs

- Dialogs are `Window` subclasses located in the `Dialogs/` folder
- Set fixed `Height` and `Width` suitable for the form content
- Dialog ViewModel is assigned via DI and set as `DataContext` in code-behind
- Close dialog by setting `DialogResult` in the ViewModel via a bound action — not directly in XAML

## Code-Behind Rules

- Code-behind (`*.xaml.cs`) must contain **only**:
  - Constructor with `InitializeComponent()` and `DataContext` assignment
  - Event handlers that are **purely view-specific** (e.g., focus management) with no business logic
- ❌ Do not place business logic, service calls, or data manipulation in code-behind
- ❌ Do not subscribe to ViewModel events in code-behind unless strictly necessary for view transitions

## Comments

- Use `<!-- Section Name -->` comments to label major layout regions (e.g., `<!-- Toolbar -->`, `<!-- Customers -->`, `<!-- Orders -->`)
- Do not comment obvious bindings or standard properties

## Common Pitfalls

- ❌ Do not use `AutoGenerateColumns="True"` on `DataGrid`
- ❌ Do not hardcode user-facing text — always use `{x:Static res:Strings.KeyName}` from the resource files
- ❌ Do not add a key to only one `.resx` file — always add to both `Strings.resx` and `Strings.es.resx`
- ❌ Do not use `x:Name` to access controls in code-behind for data operations
- ❌ Do not use `ElementName` bindings when a ViewModel property can serve the same purpose
- ❌ Do not forget `mc:Ignorable="d"` when using design-time attributes
