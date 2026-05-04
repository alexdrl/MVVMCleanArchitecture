using System.Windows;

using WpfAppCleanArchitecture.ViewModels;

namespace WpfAppCleanArchitecture.Dialogs;

/// <summary>
/// Interaction logic for OrderDialog.xaml
/// </summary>
public partial class OrderDialog : Window
{
    private readonly OrderDialogViewModel _viewModel;

    public OrderDialogViewModel ViewModel => _viewModel;

    public OrderDialog()
    {
        InitializeComponent();

        _viewModel = new OrderDialogViewModel();
        DataContext = _viewModel;

        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(OrderDialogViewModel.DialogResult))
                DialogResult = _viewModel.DialogResult;
        };
    }
}
