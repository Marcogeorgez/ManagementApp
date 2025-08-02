namespace ManagementApp.Services.Core;

public class UndoRedoService
{
    private readonly List<SwapAction> actions = [];
    private int currentIndex = -1;

    public record SwapAction(int ProjectId, string ProjectName, string TargetName, int SourceReorderId, int TargetReorderId, DateTime Time);
    public void AddSwap(int projectId, string projectName, string targetName, int sourceReorderId, int targetReorderId)
    {
        // Remove any redo actions
        if (currentIndex < actions.Count - 1)
            actions.RemoveRange(currentIndex + 1, actions.Count - currentIndex - 1);

        // Add new action
        actions.Add(new SwapAction(projectId, projectName, targetName, sourceReorderId, targetReorderId, DateTime.Now));
        currentIndex++;

        // Clean old actions
        CleanOldActions();
    }

    public SwapAction? Undo()
    {
        CleanOldActions();

        if (currentIndex < 0)
            return null;

        var action = actions[currentIndex];
        actions[currentIndex] = action with { Time = DateTime.Now }; // Reset timer
        currentIndex--;

        return action;
    }
    public SwapAction? UndoText()
    {
        CleanOldActions();

        if (currentIndex < 0)
            return null;

        return actions[currentIndex];
    }

    public SwapAction? Redo()
    {
        CleanOldActions();

        if (currentIndex >= actions.Count - 1)
            return null;

        currentIndex++;
        var action = actions[currentIndex];
        actions[currentIndex] = action with { Time = DateTime.Now }; // Reset timer

        return action;
    }
    public SwapAction? RedoText()
    {
        CleanOldActions();

        if (currentIndex >= actions.Count - 1)
            return null;

        return actions[currentIndex + 1];
    }

    public void CleanOldActions()
    {
        var oneMinuteAgo = DateTime.Now.AddMinutes(-1);
        var validActions = actions.Where(a => a.Time >= oneMinuteAgo).ToList();

        if (validActions.Count != actions.Count)
        {
            actions.Clear();
            actions.AddRange(validActions);
            currentIndex = actions.Count - 1;
        }
    }
}
