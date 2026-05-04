using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Wpf.Application.DTOs;

namespace WpfAppCleanArchitecture.ViewModels;

public partial class OrderDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private bool dialogResult;

    public bool CanSave => !string.IsNullOrWhiteSpace(Description);

    public OrderDto BuildDto(int customerId) => new()
    {
        CustomerId = customerId,
        Description = Description
    };

    partial void OnDescriptionChanged(string value)
    {
        OnPropertyChanged(nameof(CanSave));
    }

    [RelayCommand]
    private void Save()
    {
        DialogResult = true;
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogResult = false;
    }
}
