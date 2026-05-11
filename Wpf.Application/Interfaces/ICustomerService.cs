using Wpf.Application.Common;
using Wpf.Application.DTOs;

namespace Wpf.Application.Interfaces;

public interface ICustomerService
{
    ValueTask<IReadOnlyList<CustomerDto>> GetAllCustomersAsync(CancellationToken cancellationToken);
    ValueTask<IReadOnlyList<OrderDto>> GetOrdersByCustomerIdAsync(int customerId, CancellationToken cancellationToken);
    ValueTask<int> AddCustomerAsync(CustomerDto dto, CancellationToken token);
    ValueTask UpdateCustomerAsync(CustomerDto dto, CancellationToken token);
    ValueTask<Result<bool>> DeleteCustomerAsync(int id, CancellationToken token);
    ValueTask<int> AddOrderAsync(OrderDto dto, CancellationToken token);
    ValueTask<IReadOnlyList<CustomerDto>> SearchCustomersAsync(string searchTerm, CancellationToken cancellationToken);
}