using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Wpf.Application.DTOs;

using WpfAppCleanArchitecture.Services;

namespace WpfAppCleanArchitecture.ViewModels;

public partial class CustomerDialogViewModel : ObservableObject
{
    private readonly ILoadingService _loadingService;

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

    public CustomerDialogViewModel(CustomerDto customerDto, ILoadingService loadingService)
    {
        _loadingService = loadingService;
        Customer = customerDto;
        Name = customerDto.Name;
        LastName = customerDto.LastName;

        // Notify the main window that dialog data is being prepared
        _loadingService.Show();
    }

    [RelayCommand]
    private void Save()
    {
        Customer.Name = Name;
        Customer.LastName = LastName;
        CloseDialog();
        DialogResult = true;
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseDialog();
        DialogResult = false;
    }

    /// <summary>
    /// Releases the loading state. Safe to call multiple times.
    /// </summary>
    public void CloseDialog()
    {
        if (_isHidden) return;
        _isHidden = true;
        _loadingService.Hide();
    }

    private bool _isHidden;
}
