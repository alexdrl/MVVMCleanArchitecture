using System.ComponentModel;
using System.Threading;

namespace WpfAppCleanArchitecture.Services;

public class LoadingService : ILoadingService
{
    private int _counter;

    public bool IsLoading => _counter > 0;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Show()
    {
        if (Interlocked.Increment(ref _counter) == 1)
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoading)));
    }

    public void Hide()
    {
        if (Interlocked.Decrement(ref _counter) == 0)
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoading)));
    }
}
