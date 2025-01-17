/// <summary>
/// Manages the current project state throughout the Blazor application lifecycle.
/// Acts as a central store for the currently selected/active project, implementing
/// an observable pattern to notify components of state changes.
/// </summary>
public class ProjectState
{
    /// <summary>
    /// Holds the reference to the current active project.
    /// Null if no project is selected.
    /// Private set to ensures state changes only occur through the SetProject method.
    /// </summary>
    public Project? CurrentProject { get; private set; }

    /// <summary>
    /// Event that components can subscribe to for receiving notifications
    /// when the current project changes. Used in Blazor components to trigger
    /// UI updates when project state changes.
    /// </summary>
    public event Action OnChange;

    /// <summary>
    /// Updates the current project and notifies all subscribers of the change.
    /// Used when switching between projects or updating project details.
    /// </summary>
    /// <param name="project">The new project to set as current</param>
    public void SetProject(Project project)
    {
        CurrentProject = project;
        NotifyStateChanged();
    }

    /// <summary>
    /// Private helper method that triggers the OnChange event.
    /// Uses null-conditional operator to safely invoke the event
    /// only if there are subscribers.
    /// </summary>
    private void NotifyStateChanged() => OnChange?.Invoke();
}