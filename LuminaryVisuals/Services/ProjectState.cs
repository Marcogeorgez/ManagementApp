public class ProjectState
{
    public Project? CurrentProject { get; private set; }
    public event Action OnChange;

    public void SetProject(Project project)
    {
        CurrentProject = project;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
