using FluentValidation;

using Microsoft.EntityFrameworkCore;

using Wpf.Application.Common;
using Wpf.Application.DTOs;
using Wpf.Application.Interfaces;
using Wpf.Domain.Entities;
using Wpf.Infrastructure.Data;
using Wpf.Infrastructure.Mapping;

namespace Wpf.Infrastructure.Services;

public class CustomerService(
    IDbContextFactory<AppDbContext> dbContextFactory,
    IValidator<CustomerDto> validator,
    IValidator<OrderDto> orderValidator,
    CustomerMapper customerMapper,
    OrderMapper orderMapper) : ICustomerService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory = dbContextFactory;
    private readonly CustomerMapper _customerMapper = customerMapper;
    private readonly OrderMapper _orderMapper = orderMapper;
    private readonly IValidator<CustomerDto> _customerValidator = validator;
    private readonly IValidator<OrderDto> _orderValidator = orderValidator;

    public async ValueTask<IReadOnlyList<CustomerDto>> GetAllCustomersAsync(CancellationToken cancellationToken)
    {
        // Delay to demonstrate cancellation support
        await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
        using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entities = await db.Customers
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return _customerMapper.ToDtoList(entities);
    }

    public async ValueTask<IReadOnlyList<OrderDto>> GetOrdersByCustomerIdAsync(int customerId, CancellationToken cancellationToken)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var orders = await db.Orders
            .AsNoTracking()
            .Where(o => o.CustomerId == customerId)
            .ToListAsync(cancellationToken);

        return _orderMapper.ToDtoList(orders);
    }

    public async ValueTask<int> AddCustomerAsync(CustomerDto dto, CancellationToken token)
    {
        var result = _customerValidator.Validate(dto);
        if (!result.IsValid)
            throw new ValidationException(result.Errors);

        using var db = await _dbContextFactory.CreateDbContextAsync(token);
        var customer = new Customer(dto.Name, dto.LastName);
        db.Customers.Add(customer);
        await db.SaveChangesAsync(token);
        return customer.Id;
    }

    public async ValueTask UpdateCustomerAsync(CustomerDto dto, CancellationToken token)
    {
        var result = _customerValidator.Validate(dto);
        if (!result.IsValid)
            throw new ValidationException(result.Errors);

        using var db = await _dbContextFactory.CreateDbContextAsync(token);
        var customer = await db.Customers.FindAsync([dto.Id], token);
        
        if (customer is null) 
            throw new Exception("Customer not found.");

        customer.Rename(dto.Name, dto.LastName);
        await db.SaveChangesAsync(token);
    }

    public async ValueTask<Result<bool>> DeleteCustomerAsync(int id, CancellationToken token)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync(token);
        var entity = await db.Customers
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.Id == id, token);

        if (entity is null)
            return Result<bool>.Failure("Customer not found.");

        if (entity.Name.Equals("Alejandro", StringComparison.OrdinalIgnoreCase))
            return Result<bool>.Failure($"Customer '{entity.Name} {entity.LastName}' cannot be deleted.");

        if (entity.Orders.Count > 0)
            return Result<bool>.Failure($"Cannot delete customer '{entity.Name} {entity.LastName}' because they have {entity.Orders.Count} associated order(s).");

        db.Customers.Remove(entity);
        await db.SaveChangesAsync(token);
        return Result<bool>.Success(true);
    }

    public async ValueTask<int> AddOrderAsync(OrderDto dto, CancellationToken token)
    {
        var result = _orderValidator.Validate(dto);
        if (!result.IsValid)
            throw new ValidationException(result.Errors);

        using var db = await _dbContextFactory.CreateDbContextAsync(token);
        var customer = await db.Customers
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.Id == dto.CustomerId, token);

        if (customer is null)
            throw new InvalidOperationException("Customer not found.");

        customer.AddOrder(dto.Description);
        await db.SaveChangesAsync(token);
        return customer.Orders.Last().Id;
    }

    public async ValueTask<IReadOnlyList<CustomerDto>> SearchCustomersAsync(string searchTerm, CancellationToken cancellationToken)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var term = searchTerm?.Trim() ?? string.Empty;
        var entities = await db.Customers
            .AsNoTracking()
            .Where(c => c.Name.Contains(term) || c.LastName.Contains(term))
            .ToListAsync(cancellationToken);

        return _customerMapper.ToDtoList(entities);
    }
}
