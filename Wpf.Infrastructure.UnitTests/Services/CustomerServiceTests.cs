using FluentValidation;
using FluentValidation.Results;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Wpf.Application.DTOs;
using Wpf.Application.Interfaces;
using Wpf.Domain.Entities;
using Wpf.Infrastructure.Data;
using Wpf.Infrastructure.Mapping;
using Wpf.Infrastructure.Services;

namespace Wpf.Infrastructure.UnitTests.Services;

[TestClass]
public class CustomerServiceTests
{
    private Mock<IValidator<CustomerDto>> _customerValidatorMock = null!;
    private Mock<IValidator<OrderDto>> _orderValidatorMock = null!;
    private Mock<CustomerMapper> _customerMapperMock = null!;
    private Mock<OrderMapper> _orderMapperMock = null!;
    private SqliteConnection _connection = null!;
    private DbContextOptions<AppDbContext> _contextOptions = null!;

    [TestInitialize]
    public void Setup()
    {
        // Create and open in-memory SQLite connection
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        // Configure DbContext to use in-memory SQLite
        _contextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        // Create database schema using migrations
        using var context = new AppDbContext(_contextOptions);
        context.Database.Migrate();

        // Setup mocks
        _customerValidatorMock = new Mock<IValidator<CustomerDto>>();
        _orderValidatorMock = new Mock<IValidator<OrderDto>>();
        _customerMapperMock = new Mock<CustomerMapper>();
        _orderMapperMock = new Mock<OrderMapper>();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _connection?.Close();
        _connection?.Dispose();
    }

    [TestMethod]
    public async Task AddOrderAsync_ValidOrder_CreatesOrderAndReturnsId()
    {
        // Arrange
        var dbContextFactory = new TestDbContextFactory(_contextOptions);
        var service = new CustomerService(
            dbContextFactory,
            _customerValidatorMock.Object,
            _orderValidatorMock.Object,
            _customerMapperMock.Object,
            _orderMapperMock.Object);

        int customerId;
        // Create a customer in the database
        using (var context = new AppDbContext(_contextOptions))
        {
            var customer = new Customer("John", "Doe");
            context.Customers.Add(customer);
            await context.SaveChangesAsync();
            customerId = customer.Id;
        }

        var orderDto = new OrderDto
        {
            CustomerId = customerId,
            Description = "Test Order"
        };

        _orderValidatorMock.Setup(v => v.Validate(orderDto))
            .Returns(new ValidationResult());

        // Act
        var result = await service.AddOrderAsync(orderDto, CancellationToken.None);

        // Assert
        // Verify that an order was created in the database
        using (var context = new AppDbContext(_contextOptions))
        {
            var orders = await context.Orders.ToListAsync();
#pragma warning disable MSTEST0037 // Use 'Assert.HasCount' instead of 'Assert.AreEqual'
            Assert.AreEqual(1, orders.Count, "Expected exactly one order in the database");
#pragma warning restore MSTEST0037
            var order = orders[0];
            
            Assert.AreEqual("Test Order", order.Description);
            Assert.AreEqual(customerId, order.CustomerId);
        }

        _orderValidatorMock
            .Verify(v => v.Validate(orderDto), Times.Once, "Expected order validator to be called once");
    }

    [TestMethod]
    public async Task AddOrderAsync_InvalidOrderDto_ThrowsValidationException()
    {
        // Arrange
        var dbContextFactory = new TestDbContextFactory(_contextOptions);
        var service = new CustomerService(
            dbContextFactory,
            _customerValidatorMock.Object,
            _orderValidatorMock.Object,
            _customerMapperMock.Object,
            _orderMapperMock.Object);

        var orderDto = new OrderDto
        {
            CustomerId = 1,
            Description = ""
        };

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Description", "Description is required")
        };
        _orderValidatorMock.Setup(v => v.Validate(orderDto))
            .Returns(new ValidationResult(validationFailures));

        // Act & Assert
        var exception = false;
        try
        {
            await service.AddOrderAsync(orderDto, CancellationToken.None);
        }
        catch (ValidationException)
        {
            exception = true;
        }
        
        Assert.IsTrue(exception, "Expected ValidationException to be thrown");
        _orderValidatorMock.Verify(v => v.Validate(orderDto), Times.Once);
    }

    [TestMethod]
    public async Task AddOrderAsync_CustomerNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var dbContextFactory = new TestDbContextFactory(_contextOptions);
        var service = new CustomerService(
            dbContextFactory,
            _customerValidatorMock.Object,
            _orderValidatorMock.Object,
            _customerMapperMock.Object,
            _orderMapperMock.Object);

        var orderDto = new OrderDto
        {
            CustomerId = 999,
            Description = "Test Order"
        };

        _orderValidatorMock.Setup(v => v.Validate(orderDto))
            .Returns(new ValidationResult());

        // Act & Assert
        InvalidOperationException? exception = null;
        try
        {
            await service.AddOrderAsync(orderDto, CancellationToken.None);
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }
        
        Assert.IsNotNull(exception, "Expected InvalidOperationException to be thrown");
        Assert.AreEqual("Customer not found.", exception.Message);
        _orderValidatorMock.Verify(v => v.Validate(orderDto), Times.Once);
    }

    [TestMethod]
    public async Task AddOrderAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var dbContextFactory = new TestDbContextFactory(_contextOptions);
        var service = new CustomerService(
            dbContextFactory,
            _customerValidatorMock.Object,
            _orderValidatorMock.Object,
            _customerMapperMock.Object,
            _orderMapperMock.Object);

        var orderDto = new OrderDto
        {
            CustomerId = 1,
            Description = "Test Order"
        };

        _orderValidatorMock.Setup(v => v.Validate(orderDto))
            .Returns(new ValidationResult());

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        var exception = false;
        try
        {
            await service.AddOrderAsync(orderDto, cts.Token);
        }
        catch (OperationCanceledException)
        {
            exception = true;
        }
        
        Assert.IsTrue(exception, "Expected OperationCanceledException to be thrown");
        _orderValidatorMock.Verify(v => v.Validate(orderDto), Times.Once);
    }

    [TestMethod]
    public async Task AddOrderAsync_MultipleOrders_AddsAllOrdersToCustomer()
    {
        // Arrange
        var dbContextFactory = new TestDbContextFactory(_contextOptions);
        var service = new CustomerService(
            dbContextFactory,
            _customerValidatorMock.Object,
            _orderValidatorMock.Object,
            _customerMapperMock.Object,
            _orderMapperMock.Object);

        int customerId;
        // Create a customer in the database
        using (var context = new AppDbContext(_contextOptions))
        {
            var customer = new Customer("John", "Doe");
            context.Customers.Add(customer);
            await context.SaveChangesAsync();
            customerId = customer.Id;
        }

        var orderDto1 = new OrderDto
        {
            CustomerId = customerId,
            Description = "First Order"
        };

        var orderDto2 = new OrderDto
        {
            CustomerId = customerId,
            Description = "Second Order"
        };

        _orderValidatorMock.Setup(v => v.Validate(It.IsAny<OrderDto>()))
            .Returns(new ValidationResult());

        // Act
        await service.AddOrderAsync(orderDto1, CancellationToken.None);
        await service.AddOrderAsync(orderDto2, CancellationToken.None);

        // Assert
        using (var context = new AppDbContext(_contextOptions))
        {
            var customer = await context.Customers
                .Include(c => c.Orders)
                .FirstAsync(c => c.Id == customerId);
            
#pragma warning disable MSTEST0037 // Use 'Assert.HasCount' instead of 'Assert.AreEqual'
            Assert.AreEqual(2, customer.Orders.Count);
#pragma warning restore MSTEST0037
            Assert.IsTrue(customer.Orders.Any(o => o.Description == "First Order"));
            Assert.IsTrue(customer.Orders.Any(o => o.Description == "Second Order"));
        }

        _orderValidatorMock.Verify(v => v.Validate(It.IsAny<OrderDto>()), Times.Exactly(2));
    }

    private class TestDbContextFactory : IDbContextFactory<AppDbContext>
    {
        private readonly DbContextOptions<AppDbContext> _options;

        public TestDbContextFactory(DbContextOptions<AppDbContext> options)
        {
            _options = options;
        }

        public AppDbContext CreateDbContext()
        {
            return new AppDbContext(_options);
        }

        public ValueTask<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        {
            return new ValueTask<AppDbContext>(new AppDbContext(_options));
        }
    }
}
