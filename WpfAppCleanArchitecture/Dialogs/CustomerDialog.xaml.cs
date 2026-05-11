using System.Windows;

using Wpf.Application.DTOs;

using WpfAppCleanArchitecture.Services;
using WpfAppCleanArchitecture.ViewModels;

namespace WpfAppCleanArchitecture.Dialogs;

/// <summary>
/// Interaction logic for CustomerDialog.xaml
/// </summary>
public partial class CustomerDialog : Window
{
    public CustomerDto Customer => _viewModel.Customer;

    private readonly CustomerDialogViewModel _viewModel;

    public CustomerDialog(CustomerDto customer, ILoadingService loadingService)
    {
        InitializeComponent();

        _viewModel = new CustomerDialogViewModel(customer, loadingService);

        DataContext = _viewModel;

        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(CustomerDialogViewModel.DialogResult) && _viewModel.DialogResult)
                DialogResult = true;
            else if (e.PropertyName == nameof(CustomerDialogViewModel.DialogResult) && !_viewModel.DialogResult)
                DialogResult = false;
        };

        Closing += (_, _) => _viewModel.CloseDialog();
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        if (_viewModel.CanSave)
        {
            DialogResult = true;
        }
    }
}
