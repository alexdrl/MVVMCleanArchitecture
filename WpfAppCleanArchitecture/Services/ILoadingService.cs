using System.ComponentModel;

namespace WpfAppCleanArchitecture.Services;

/// <summary>
/// Shared service that tracks loading state across ViewModels.
/// Supports nested calls via an internal counter: IsLoading is true
/// until every Show() call has a matching Hide() call.
/// </summary>
public interface ILoadingService : INotifyPropertyChanged
{
    bool IsLoading { get; }
    void Show();
    void Hide();
}
