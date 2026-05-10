---
name: 'Test Standards'
description: 'Conventions for test projects in this solution'
applyTo: '**/*.Tests/**/*.cs,**/UnitTests/**/*.cs,**/*Tests.cs'
---

# Test Project Conventions

## Framework & Libraries

| Purpose | Library | Notes |
|---|---|---|
| Test framework | **MSTest v4** (`MSTest` 4.x) | `[TestClass]`, `[TestMethod]`, `[TestInitialize]`, `[TestCleanup]` |
| Mocking | **Moq** 4.x | `Mock<T>`, `.Setup()`, `.Returns()`, `It.IsAny<T>()` |
| In-memory database | **SQLite in-memory** via `Microsoft.Data.Sqlite` | One `SqliteConnection("DataSource=:memory:")` per test class |
| ORM | **EF Core** `Microsoft.EntityFrameworkCore.Sqlite` | Schema created with `context.Database.Migrate()` |

## Project Layout

- One test project per production project: `[ProjectName].UnitTests`
- Mirror the source namespace: `Wpf.Infrastructure.Services.CustomerService` → `Wpf.Infrastructure.UnitTests.Services.CustomerServiceTests`
- Test file name matches class name: `CustomerServiceTests.cs`

## Class Structure

```csharp
[TestClass]
public class CustomerServiceTests
{
    // --- mocks (null! – initialized in Setup) ---
    private Mock<IValidator<CustomerDto>> _customerValidatorMock = null!;
    private Mock<IValidator<OrderDto>>    _orderValidatorMock    = null!;
    private Mock<CustomerMapper>          _customerMapperMock    = null!;
    private Mock<OrderMapper>             _orderMapperMock       = null!;

    // --- in-memory SQLite ---
    private SqliteConnection            _connection     = null!;
    private DbContextOptions<AppDbContext> _contextOptions = null!;

    [TestInitialize]
    public void Setup()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _contextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new AppDbContext(_contextOptions);
        context.Database.Migrate();          // apply real EF Core migrations

        _customerValidatorMock = new Mock<IValidator<CustomerDto>>();
        _orderValidatorMock    = new Mock<IValidator<OrderDto>>();
        _customerMapperMock    = new Mock<CustomerMapper>();
        _orderMapperMock       = new Mock<OrderMapper>();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _connection?.Close();
        _connection?.Dispose();
    }
}
```

## Naming Convention

`MethodName_Scenario_ExpectedBehavior`

| Segment | Examples |
|---|---|
| `MethodName` | `AddOrderAsync`, `DeleteCustomerAsync` |
| `Scenario` | `ValidOrder`, `CustomerNotFound`, `InvalidOrderDto`, `CancellationRequested` |
| `ExpectedBehavior` | `ReturnsId`, `ThrowsValidationException`, `ThrowsInvalidOperationException` |

## In-Memory SQLite Pattern

- Open **one** `SqliteConnection` in `[TestInitialize]`; close & dispose in `[TestCleanup]`.
- Apply schema once per test via `context.Database.Migrate()` — uses real EF Core migrations, not `EnsureCreated`.
- Seed data directly with `AppDbContext` before calling the service under test.
- Verify side-effects with a fresh `AppDbContext` instance **after** the act step.
- Never share a `DbContext` instance between arrange, act, and assert sections.

```csharp
// Seed
using (var context = new AppDbContext(_contextOptions))
{
    context.Customers.Add(new Customer("John", "Doe"));
    await context.SaveChangesAsync();
}

// Assert (separate context)
using (var context = new AppDbContext(_contextOptions))
{
    var orders = await context.Orders.ToListAsync();
    Assert.HasCount(1, orders);
}
```

## TestDbContextFactory (inner helper class)

Every test class that exercises a service depending on `IDbContextFactory<AppDbContext>` must declare this private inner class:

```csharp
private sealed class TestDbContextFactory : IDbContextFactory<AppDbContext>
{
    private readonly DbContextOptions<AppDbContext> _options;

    public TestDbContextFactory(DbContextOptions<AppDbContext> options)
        => _options = options;

    public AppDbContext CreateDbContext() => new(_options);

    public ValueTask<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        => new(new AppDbContext(_options));
}
```

## Mocking Rules

- Mock only **external dependencies** (`IValidator<T>`, mappers). Never mock code under test.
- For **valid** paths, set up validators to return an empty `new ValidationResult()`.
- For **invalid** paths, provide a `List<ValidationFailure>` and wrap it: `new ValidationResult(failures)`.
- Use `It.IsAny<T>()` only when the exact argument is irrelevant to the scenario being tested.

```csharp
// Valid path
_orderValidatorMock.Setup(v => v.Validate(orderDto)).Returns(new ValidationResult());

// Invalid path
_orderValidatorMock.Setup(v => v.Validate(orderDto))
    .Returns(new ValidationResult(new List<ValidationFailure>
    {
        new("Description", "Description is required")
    }));
```

## Assertions

- Prefer `Assert.HasCount(expected, collection)` (MSTest 4) over `Assert.AreEqual(n, col.Count)`.
- Throw-based assertions: use `await Assert.ThrowsExactlyAsync<TException>(...)` — do **not** use try/catch to detect exceptions.
- One logical assertion per test; use `[DataTestMethod]` + `[DataRow]` for multiple input variants.

```csharp
// Exception assertion
await Assert.ThrowsExactlyAsync<ValidationException>(
    () => service.AddOrderAsync(dto, CancellationToken.None));

// Collection count
Assert.HasCount(2, orders);
```

## Service Instantiation

Always construct the service under test inline in each `[TestMethod]` (not in `[TestInitialize]`), so each test owns its own instance:

```csharp
var service = new CustomerService(
    new TestDbContextFactory(_contextOptions),
    _customerValidatorMock.Object,
    _orderValidatorMock.Object,
    _customerMapperMock.Object,
    _orderMapperMock.Object);
```

## What NOT to Do

- ❌ Don't use `EnsureCreated()` — always use `Migrate()` to exercise real migrations.
- ❌ Don't share `DbContext` instances across Arrange / Act / Assert.
- ❌ Don't use `try/catch` to assert exceptions — use `Assert.ThrowsExactlyAsync`.
- ❌ Don't suppress `MSTEST0037` — use `Assert.HasCount` instead.
- ❌ Don't mock types that live in the same solution (e.g., domain entities, `CustomerService` itself).
- ❌ Don't add `[TestInitialize]` logic specific to a single test — keep setup generic.
