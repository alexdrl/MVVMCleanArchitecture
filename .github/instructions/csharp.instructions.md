---
name: 'C# Standards'
description: 'Coding conventions for C# files'
applyTo: '**/*.cs'
---

# C# Coding Conventions

## General

- Target framework: **.NET 8**
- Language: **C# latest** (file-scoped namespaces, primary constructors, collection expressions)
- Encoding: **UTF-8**, line endings: **CRLF**

## Naming

| Element | Convention | Example |
|---|---|---|
| Classes / Interfaces | PascalCase | `CustomerService`, `ICustomerService` |
| Methods | PascalCase | `GetAllCustomersAsync` |
| Properties | PascalCase | `Customer.Name` |
| Private fields | camelCase with `_` prefix | `_dbContextFactory` |
| Local variables / parameters | camelCase | `customerId`, `cancellationToken` |
| Constants | PascalCase | `MaxNameLength` |
| Entities | Singular noun | `Customer`, `Order` |
| DTOs | Entity name + `Dto` suffix | `CustomerDto`, `OrderDto` |
| Validators | Entity/DTO name + `Validator` suffix | `CustomerValidator` |
| Mappers | Entity name + `Mapper` suffix | `CustomerMapper` |

## File & Namespace Organization

- Use **file-scoped namespaces**: `namespace Wpf.Domain.Entities;`
- One class per file; file name matches class name
- Group `using` directives alphabetically; no unused usings

## Types & Access Modifiers

- Prefer `public` only when needed; default to `internal` for infrastructure types
- Entity properties: **protected setters** to enforce encapsulation
  ```csharp
  public string Name { get; protected set; } = string.Empty;
  ```
- Initialize string properties with `string.Empty`, collections with `[]`

## Collections

- Use collection expressions `[]` instead of `new List<T>()` for initializers
  ```csharp
  public ICollection<Order> Orders { get; set; } = [];
  ```
- Return `IReadOnlyList<T>` from service methods
- Use `AsNoTracking()` for all read-only EF Core queries

## Async/Await

- Use **`ValueTask`** (not `Task`) for service interface methods
- Always accept `CancellationToken` as last parameter, named `cancellationToken` or `token`
- Suffix all async methods with `Async`
  ```csharp
  ValueTask<IReadOnlyList<CustomerDto>> GetAllCustomersAsync(CancellationToken cancellationToken);
  ```

## Constructors & Dependency Injection

- Prefer **primary constructors** for service classes
  ```csharp
  public class CustomerService(
      IDbContextFactory<AppDbContext> dbContextFactory,
      IValidator<CustomerDto> validator,
      CustomerMapper customerMapper) : ICustomerService
  ```
- Assign primary constructor parameters to `readonly` backing fields immediately
  ```csharp
  private readonly IDbContextFactory<AppDbContext> _dbContextFactory = dbContextFactory;
  ```
- Always use `IDbContextFactory<AppDbContext>` — never inject `AppDbContext` directly
- Always dispose DbContext with `using var db = await _dbContextFactory.CreateDbContextAsync(token);`

## Guard Clauses

- Validate domain invariants via Guard clauses at the top of constructors and behavior methods.
- Guard clause code is present in Wpf.Domain Guard.cs.
  ```csharp
  Guard.AgainstNullOrWhiteSpace(name, nameof(name));
  ```
- No validation logic inside domain entities themselves (use Application validators)

## Error Handling

- **Domain layer**: throw exceptions via Guard clauses for invariant violations
- **Application/Infrastructure**: throw `ValidationException` for business rule violations
- **Presentation (ViewModel)**: catch all exceptions and display via `MessageBox`; never swallow silently — see [ViewModel Standards](./viewmodels.instructions.md)

## EF Core Conventions

- Configure entities with `IEntityTypeConfiguration<T>` — never configure inline in `OnModelCreating`
- Use `HasKey`, `HasMaxLength`, `IsRequired` in configuration classes
- All migrations require `-s WpfAppCleanArchitecture` startup project flag

## Code Style

- Prefer **expression bodies** for simple one-liner methods/properties
- No inline comments for obvious code. Use XML documentation comments for public APIs only.
- No trailing whitespace; blank line at end of file
- Braces on new lines (Allman style) for methods and classes; single-line `if` without braces only for guard returns
