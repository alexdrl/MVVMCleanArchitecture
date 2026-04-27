using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System.Xml.Linq;

using Wpf.Application.DTOs;

namespace WpfAppCleanArchitecture.ViewModels;

public partial class CustomerDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private CustomerDto customer = new();

    [ObservableProperty]
    private bool dialogResult;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string lastName = string.Empty;

    public bool IsNew => Customer.Id == 0;

    public bool CanSave => !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(lastName);

    partial void OnNameChanged(string value)
    {
        OnPropertyChanged(nameof(CanSave));
    }

    partial void OnLastNameChanged(string value)
    {
        OnPropertyChanged(nameof(CanSave));
    }

    public CustomerDialogViewModel(CustomerDto customerDto)
    {
        Customer = customerDto;
        Name = customerDto.Name;
        LastName = customerDto.LastName;
    }

    [RelayCommand]
    private void Save()
    {
        Customer.Name = Name;
        Customer.LastName = LastName;
        DialogResult = true;
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogResult = false;
    }
}
