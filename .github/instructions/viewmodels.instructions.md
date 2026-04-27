---
name: 'ViewModel Standards'
description: 'Coding conventions for WPF ViewModels using CommunityToolkit.Mvvm'
applyTo: '**/*ViewModel.cs'
---

# ViewModels (CommunityToolkit.Mvvm)

## Naming

| Element | Convention | Example |
|---|---|---|
| ViewModels | Feature/Entity + `ViewModel` suffix | `MainViewModel`, `CustomerDialogViewModel` |
| Dialog ViewModels | Entity + `DialogViewModel` suffix | `CustomerDialogViewModel` |

## Observable Properties

- Use `[ObservableProperty]` for bindable properties (generates the backing field and property)
- For computed properties that depend on other observable properties (e.g., `CanSave`), always implement the corresponding `OnXxxChanged` partial method to trigger `OnPropertyChanged`:
  ```csharp
  public bool CanSave => !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(lastName);

  partial void OnNameChanged(string value)
      => OnPropertyChanged(nameof(CanSave));

  partial void OnLastNameChanged(string value)
      => OnPropertyChanged(nameof(CanSave));
  ```
- Every `[ObservableProperty]` field whose value is referenced in a computed property **must** have its own `OnXxxChanged` partial method calling `OnPropertyChanged(nameof(<ComputedProperty>))`.
- Never rely on the UI re-evaluating a computed property without an explicit notification.

## Structure & Registration

- ViewModels are **singletons** registered in DI (see `App.xaml.cs`)
- Inject required services via constructor using **primary constructors**
- Use `[RelayCommand]` for commands

## Error Handling

- **Presentation (ViewModel)**: catch all exceptions and display via `MessageBox`; never swallow silently
