---
name: addEntityProperty
description: Add a new property to a domain entity and propagate it across all Clean Architecture layers
---
Add a new property `[PropertyName]` of type `[PropertyType]` to the `[EntityName]` domain entity and propagate the change across all layers of the Clean Architecture solution.

Follow these steps in order:

## 1. Domain Layer
- Add the property to `[EntityName]` with a **protected setter** and a default value.
- Update the constructor to accept and guard the new parameter using `Guard.AgainstN
- llOrWhiteSpace` (or appropriate guard) and assign it.
- Update any behavior methods that modify the entity (e.g., `Rename`, `Update`) to also accept and apply the new property.

## 2. Application Layer
- Add the property to `[EntityName]Dto`.
- Add a validation rule for the new property in `[EntityName]Validator` using FluentValidation (`NotEmpty`, `MaximumLength`, etc.).

## 3. Infrastructure Layer
- Add the EF Core column configuration (`IsRequired`, `HasMaxLength`) for the new property in `[EntityName]Configuration` (`IEntityTypeConfiguration<[EntityName]>`).
- Update `[EntityName]Service` to pass the new property when constructing or updating the entity.
- Update `AppDbSeeder` seed data to include a representative value for the new property.

## 4. Presentation Layer (WPF / MVVM)
- Add an `[ObservableProperty]` field for the new property in `[EntityName]DialogViewModel`.
- Update `CanSave` (or equivalent computed property) to include validation for the new field.
- Add a corresponding `OnPropertyChanged(nameof(CanSave))` call — **add an `On[PropertyName]Changed` partial method** for every `[ObservableProperty]` referenced in a computed property so the UI binding updates correctly.
- Update the constructor of the ViewModel to initialize the new field from the DTO.
- Update the `Save()` method to write the new field back to the DTO.
- Add a label and input control for the new property in `[EntityName]Dialog.xaml`, adjusting the window height if needed.
- Add a new column for the new property in the relevant `DataGrid` in `MainWindow.xaml`.

## 5. Database Migration
- Run the EF Core migration command to generate and apply the schema change:
  ```bash
  dotnet ef migrations add Add[EntityName][PropertyName] -p [InfrastructureProject] -s [StartupProject]
  ```

## Notes
- Always build and verify compilation succeeds after all changes.
- Every `[ObservableProperty]` whose value is used in a computed property **must** have its own `OnXxxChanged` partial method triggering `OnPropertyChanged(nameof(<ComputedProperty>))`. Without it, buttons bound to the computed property will never enable/disable correctly.
