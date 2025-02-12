using MudBlazor;

public class BreakpointService
{
    public event Action OnChange;
    private Breakpoint _currentBreakpoint;

    public Breakpoint CurrentBreakpoint
    {
        get => _currentBreakpoint;
        set
        {
            if (_currentBreakpoint != value)
            {
                _currentBreakpoint = value;
                OnChange?.Invoke(); // Notify subscribers
            }
        }
    }
}
