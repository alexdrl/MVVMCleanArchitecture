# WPF Clean Architecture Instructions

## Architecture Overview

This is a **WPF desktop application** following **Clean Architecture** with strict layer separation:

```
SharedKernel/          → Shared abstractions (IAggregateRoot)
Wpf.Domain/           → Entities, domain logic, Guard clauses
Wpf.Application/      → DTOs, Service interfaces, FluentValidation validators
Wpf.Infrastructure/   → EF Core, SQLite, Mapperly mappers, Service implementations
WpfAppCleanArchitecture/ → WPF UI, ViewModels (MVVM), Dialogs
```

**Dependency Flow**: UI → Infrastructure → Application → Domain → SharedKernel

## Key Patterns

### Domain Layer
- **Entities** inherit from `BaseEntity<TId>` and implement `IAggregateRoot` for aggregates
- Use **protected setters** on entity properties (e.g., `Customer.Name`)
- Domain logic in **behavior methods** (e.g., `Customer.Rename()`, `Customer.AddOrder()`)
- **Guard clauses** validate invariants: `Guard.AgainstNullOrWhiteSpace(name, nameof(name))`
- Example: `Wpf.Domain/Entities/Customer.cs`

### Application Layer
- **DTOs** for data transfer (e.g., `CustomerDto`, `OrderDto`)
- **Service interfaces** define contracts (e.g., `ICustomerService`)
- **FluentValidation** validators in `Wpf.Application/Validaton/` namespace
- Example validator pattern:
  ```csharp
  public class CustomerValidator : AbstractValidator<CustomerDto>
  {
      public CustomerValidator()
      {
          RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
      }
  }
  ```

### Infrastructure Layer
- **IDbContextFactory<AppDbContext>** for safe DI + design-time scenarios
- Always use `using var db = await _dbContextFactory.CreateDbContextAsync(cancellation)`
- **Mapperly** for entity↔DTO mapping: `[Mapper] public partial class CustomerMapper`
- **IEntityTypeConfiguration** for EF Core config (see `OrderConfiguration.cs`)
- Services validate DTOs before operations and throw `ValidationException` on failure

### Presentation Layer (MVVM)
- **CommunityToolkit.Mvvm**: `ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`
- ViewModels are **singletons** registered in DI (see `App.xaml.cs`)
- **Dialogs** for add/edit operations return `bool?` via `ShowDialog()`
- Validation errors displayed via `MessageBox` with formatted messages
- Example: `MainViewModel.ShowValidationErrors()` formats FluentValidation errors

## Dependency Injection Setup

All DI configured in `App.xaml.cs` using `Host.CreateDefaultBuilder()`:
- **DbContext**: Both `AddDbContext` and `AddDbContextFactory` for flexibility
- **Validators**: Singleton `IValidator<CustomerDto>`, `IValidator<OrderDto>`
- **Services**: Scoped `ICustomerService`
- **Mappers**: Singleton `CustomerMapper`, `OrderMapper`
- **ViewModels**: Singleton `MainViewModel` with MainWindow factory

## Database & Migrations

- **SQLite** with connection string: `Data Source=app.db`
- **Auto-migration** on startup: `db.Database.Migrate()` in `App.OnStartup()`
- Seeding via `AppDbSeeder.Seed(db)` after migration

### Running Migrations
```bash
# Add new migration
dotnet ef migrations add MigrationName -s WpfAppCleanArchitecture

# Update database
dotnet ef database update -s WpfAppCleanArchitecture
```

**Note**: Startup project must be specified with `-s` flag

## Conventions

### Naming
- Entities: `Customer`, `Order` (singular, in `Wpf.Domain/Entities/`)
- DTOs: `CustomerDto`, `OrderDto` (in `Wpf.Application/DTOs/`)
- Services: `ICustomerService` (interface in Application), `CustomerService` (impl in Infrastructure)
- ViewModels: `MainViewModel`, `CustomerDialogViewModel` (in `WpfAppCleanArchitecture/ViewModels/`)

### Async/Await
- Use `ValueTask` for service methods (EF Core optimization)
- Always pass `CancellationToken` to async methods
- Name async methods with `Async` suffix

### Error Handling
- **Domain**: Throw exceptions for invariant violations via Guard clauses
- **Application**: Throw `ValidationException` for business rule violations
- **UI**: Catch exceptions in ViewModels, display via `MessageBox`

## Adding New Features

### New Entity
1. Create entity in `Wpf.Domain/Entities/` inheriting `BaseEntity<TId>`
2. Add DbSet to `AppDbContext`
3. Create `IEntityTypeConfiguration` in `Wpf.Infrastructure/Data/Configuration/`
4. Create migration: `dotnet ef migrations add AddEntityName -s WpfAppCleanArchitecture`

### New Service Operation
1. Define method in service interface (`Wpf.Application/Interfaces/`)
2. Implement in service class (`Wpf.Infrastructure/Services/`)
3. Create DTO if needed (`Wpf.Application/DTOs/`)
4. Add Mapperly mapper method (`Wpf.Infrastructure/Mapping/`)
5. Create FluentValidation validator if needed (`Wpf.Application/Validaton/`)

### New ViewModel
1. Create ViewModel inheriting `ObservableObject` in `WpfAppCleanArchitecture/ViewModels/`
2. Use `[ObservableProperty]` for bindable properties
3. Use `[RelayCommand]` for commands
4. Register as singleton in `App.xaml.cs` DI configuration
5. Inject required services via constructor

## Testing & Debugging

- **Run**: `dotnet run --project WpfAppCleanArchitecture`
- **Build**: Standard Visual Studio build (F5 for debug, Ctrl+Shift+B for build)
- Build the solution after doing changes.
- Database file: `app.db` in WpfAppCleanArchitecture output directory

## Common Pitfalls

- ❌ Don't use `DbContext` directly—use `IDbContextFactory<AppDbContext>`
- ❌ Don't forget `AsNoTracking()` for read-only queries
- ❌ Don't validate in Domain—use Application validators
- ❌ Don't inject Application/Infrastructure into Domain—keep it pure
- ❌ Don't forget to call `.Migrate()` before seeding data
