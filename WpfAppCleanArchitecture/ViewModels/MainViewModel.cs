using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using FluentValidation;

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

using Wpf.Application.DTOs;
using Wpf.Application.Interfaces;

using WpfAppCleanArchitecture.Dialogs;
using WpfAppCleanArchitecture.Resources;
using WpfAppCleanArchitecture.Services;

namespace WpfAppCleanArchitecture.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ICustomerService _customerService;
    private readonly ILoadingService _loadingService;

    public MainViewModel(ICustomerService customerService, ILoadingService loadingService)
    {
        _customerService = customerService;
        _loadingService = loadingService;
    }

    public ILoadingService LoadingService => _loadingService;

    [ObservableProperty]
    private ObservableCollection<CustomerDto> customers = new();

    [ObservableProperty]
    private CustomerDto? selectedCustomer;

    [ObservableProperty]
    private ObservableCollection<OrderDto> orders = new();

    [ObservableProperty]
    private ObservableCollection<string> validationMessages = new();

    [ObservableProperty]
    private string searchTerm = string.Empty;

    [ObservableProperty]
    private string? errorMessage;

    [RelayCommand]
    public async Task SearchCustomersAsync()
    {
        ErrorMessage = null;
        _loadingService.Show();
        try
        {
            Customers.Clear();
            var result = await _customerService.SearchCustomersAsync(SearchTerm, CancellationToken.None);
            foreach (var customer in result)
                Customers.Add(customer);
        }
        catch (Exception ex)
        {
            ErrorMessage = Strings.ErrorSearchingCustomers + ex.Message;
        }
        finally
        {
            _loadingService.Hide();
        }
    }

    [RelayCommand]
    public async Task ClearSearchAsync()
    {
        SearchTerm = string.Empty;
        await LoadCustomersAsync();
    }

    partial void OnSelectedCustomerChanged(CustomerDto? value)
    {
        _ = LoadOrdersAsync(value);
        AddOrderCommand.NotifyCanExecuteChanged();
    }

    private async Task LoadOrdersAsync(CustomerDto? customer)
    {
        Orders.Clear();
        if (customer is null) return;

        ErrorMessage = null;
        _loadingService.Show();
        try
        {
            var result = await _customerService.GetOrdersByCustomerIdAsync(customer.Id, CancellationToken.None);
            foreach (var order in result)
                Orders.Add(order);
        }
        catch (Exception ex)
        {
            ErrorMessage = Strings.ErrorLoadingOrders + ex.Message;
        }
        finally
        {
            _loadingService.Hide();
        }
    }

    [RelayCommand]
    public async Task LoadCustomersAsync()
    {
        ErrorMessage = null;
        _loadingService.Show();
        try
        {
            Customers.Clear();
            var result = await _customerService.GetAllCustomersAsync(CancellationToken.None);
            foreach (var customer in result)
                Customers.Add(customer);
        }
        catch (Exception ex)
        {
            ErrorMessage = Strings.ErrorLoadingCustomers + ex.Message;
        }
        finally
        {
            _loadingService.Hide();
        }
    }

    [RelayCommand]
    public async Task AddCustomerAsync()
    {
        var newCustomer = new CustomerDto();
        var dialog = new CustomerDialog(newCustomer, _loadingService);

        if (dialog.ShowDialog() == true)
        {
            try
            {
                newCustomer.Name = dialog.Customer.Name; // Assuming dialog returns name 
                var id = await _customerService.AddCustomerAsync(newCustomer, CancellationToken.None);
                newCustomer.Id = id;

                Customers.Add(newCustomer);
            }
            catch (ValidationException ex)
            {
                ShowValidationErrors(ex);
            }
        }
    }

    [RelayCommand]
    public async Task EditCustomerAsync()
    {
        if (SelectedCustomer is null) return;

        var copy = new CustomerDto
        {
            Id = SelectedCustomer.Id,
            Name = SelectedCustomer.Name
        };

        var dialog = new CustomerDialog(copy, _loadingService);
        if (dialog.ShowDialog() == true)
        {
            try
            {
                await _customerService.UpdateCustomerAsync(copy, CancellationToken.None);
                SelectedCustomer.Name = copy.Name;
            }
            catch (ValidationException ex)
            {
                ShowValidationErrors(ex);
            }
        }
    }

    [RelayCommand]
    public async Task DeleteCustomerAsync()
    {
        if (SelectedCustomer is null) return;

        var result = MessageBox.Show($"Delete {SelectedCustomer.Name}?", "Confirm", MessageBoxButton.YesNo);
        if (result != MessageBoxResult.Yes) return;

        try
        {
            var deleteResult = await _customerService.DeleteCustomerAsync(SelectedCustomer.Id, CancellationToken.None);
            if (!deleteResult.IsSuccess)
            {
                MessageBox.Show(deleteResult.ErrorMessage, "Cannot Delete", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Customers.Remove(SelectedCustomer);
            SelectedCustomer = null;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Delete failed: " + ex.Message);
        }
    }

    private void ShowValidationErrors(ValidationException ex)
    {
        var message = string.Join("\n", ex.Errors
               .Select(error => $"• {error.PropertyName}: {error.ErrorMessage}"));

        MessageBox.Show(message, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    [RelayCommand(CanExecute = nameof(CanAddOrder))]
    public async Task AddOrderAsync()
    {
        if (SelectedCustomer is null) return;

        var dialog = new OrderDialog();
        if (dialog.ShowDialog() == true)
        {
            try
            {
                var dto = dialog.ViewModel.BuildDto(SelectedCustomer.Id);
                var id = await _customerService.AddOrderAsync(dto, CancellationToken.None);
                dto.Id = id;
                Orders.Add(dto);
            }
            catch (ValidationException ex)
            {
                ShowValidationErrors(ex);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Strings.ErrorAddingOrder + ex.Message);
            }
        }
    }

    private bool CanAddOrder() => SelectedCustomer is not null;
}