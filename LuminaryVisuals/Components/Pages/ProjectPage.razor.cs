using LuminaryVisuals.Components.ProjectPageDialogue;
using LuminaryVisuals.Components.Shared;
using LuminaryVisuals.Components.Shared.Logging;
using LuminaryVisuals.Components.Shared.ProfileComponents;
using LuminaryVisuals.Data.Entities;
using LuminaryVisuals.Models;
using LuminaryVisuals.Services.Core;
using LuminaryVisuals.Services.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using MudBlazor;
using System.Security.Claims;
using System.Text.Json;
namespace LuminaryVisuals.Components.Pages;

public partial class ProjectPage : Microsoft.AspNetCore.Components.ComponentBase
{
    private bool _loading = true;
    private bool _loadingIndicator;
    private bool isSelectorVisible;
    [CascadingParameter]
    public ClaimsPrincipal? currentUser { get; set; }
    private MudDataGrid<Project> _dataGrid = new();
    private List<Project?> projects = new List<Project?>();
    private Project? projectToDelete;
    private readonly DialogOptions DialogOptions = new() { CloseButton = true, FullScreen = false };
    private bool _isArchived;
    private bool _isAdminView;
    private bool isAdminView;
    private bool _isEditorView;
    private bool _isClientView;
    private bool _groupByClientName;
    private int timezoneOffsetMinutes; // Gets User local time to pass it to change timing of chat-messages etc according to his own.
    private string _searchString = "";
    private string _currentUserId = "";
    private string _currentRole = "";
    private string _userName = "";
    private string EditTemplateCss => _currentRole == "Client" ? "d-none" : "d-block";
    private string? CircuitId;
    private List<UserRoleViewModel.UserProjectViewModel> Editors = new List<UserRoleViewModel.UserProjectViewModel>();
    private List<UserRoleViewModel.UserProjectViewModel> Clients = new List<UserRoleViewModel.UserProjectViewModel>();
    private Dictionary<string, bool> _columnVisibility = new();
    private List<ColumnPreset> _columnPresets = new();
    [Inject] private IConfirmationService ConfirmationService { get; set; } = default!;
    [Inject] private LoadingService LoadingService { get; set; }
    private Dictionary<string, List<string>> RoleBasedColumns => new()
    {
        { "Admin", new List<string> { "InternalId", "ProjectName","ClientName","PrimaryEditor","SecondaryEditor", "ProjectDescription", "ProgressBar", "ShootDate", "DueDate", "WorkingMonth", "Status","ClientPaymentStatus","PrivateNotes","AdminBillableHoursView","ClientBillable","ClientBillableAmount","EditorBillable","SubmittedStatus","Actions","Archive" } },
        { "Editor", new List<string> { "InternalId", "ProjectName","ClientName","PrimaryEditor","SecondaryEditor", "ProjectDescription", "ProgressBar", "ShootDate", "DueDate", "WorkingMonth", "Status","PrivateNotes","BillableHours","EditorBillable","SubmittedStatus", "Actions"} },
        { "Client", new List<string> { "ExternalId", "ProjectName", "ProjectDescription", "ProgressBar", "ShootDate","ClientBillableAmount", "Status","ClientPaymentStatus" } }
    };
    private Guid _gridKey = Guid.NewGuid();
    private List<ColumnDefinition> _availableColumns = new()
    {
        new ColumnDefinition { Name = "ClientUserName", DisplayName = "Client User Name" },
        new ColumnDefinition { Name = "ProgressBar", DisplayName = "Progress Bar" },
        new ColumnDefinition { Name = "ShootDate", DisplayName = "Shoot Date" },
        new ColumnDefinition { Name = "DueDate", DisplayName = "Due Date" },
        new ColumnDefinition { Name = "WorkingMonth", DisplayName = "Working Month" },
        new ColumnDefinition { Name = "Status", DisplayName = "Status" },
        new ColumnDefinition { Name = "AdminStatus", DisplayName = "Admin Status" },
        new ColumnDefinition { Name = "Link", DisplayName = "Link" },
        new ColumnDefinition { Name = "PrimaryEditorName", DisplayName = "Primary Editor Name" },
        new ColumnDefinition { Name = "SecondaryEditorName", DisplayName = "Secondary Editor Name" },
        new ColumnDefinition { Name = "PrivateNotes", DisplayName = "Private Notes" },
        new ColumnDefinition { Name = "PrimaryEditorDatePaid", DisplayName = "Primary Editor Date Paid" },
        new ColumnDefinition { Name = "PrimaryEditorLoggedHours", DisplayName = "Primary Editor Logged Hours" },
        new ColumnDefinition { Name = "PrimaryEditorOvertime", DisplayName = "Primary Editor Overtime" },
        new ColumnDefinition { Name = "PrimaryEditorAdjustment", DisplayName = "Primary Editor Adjustment" },
        new ColumnDefinition { Name = "PrimaryEditorFinalBillableHours", DisplayName = "Primary Editor Final Billable Hours" },
        new ColumnDefinition { Name = "PrimaryEditorPayment", DisplayName = "Primary Editor Payment" },
        new ColumnDefinition { Name = "SecondaryEditorDatePaid", DisplayName = "Secondary Editor Date Paid" },
        new ColumnDefinition { Name = "SecondaryEditorLoggedHours", DisplayName = "Secondary Editor Logged Hours" },
        new ColumnDefinition { Name = "SecondaryEditorOvertime", DisplayName = "Secondary Editor Overtime" },
        new ColumnDefinition { Name = "SecondaryEditorAdjustment", DisplayName = "Secondary Editor Adjustment" },
        new ColumnDefinition { Name = "SecondaryEditorFinalBillableHours", DisplayName = "Secondary Editor Final Billable Hours" },
        new ColumnDefinition { Name = "SecondaryEditorPayment", DisplayName = "Secondary Editor Payment" },
        new ColumnDefinition { Name = "ClientBillableHours", DisplayName = "Client Billable Hours" },
        new ColumnDefinition { Name = "ClientPayment", DisplayName = "Client Payment" },
        new ColumnDefinition { Name = "ClientPaymentToggle", DisplayName = "Client Payment Toggle" },

    };
    private List<Notification> notificationList = new();
    [Inject] private UserNotificationService UserNotificationService { get; set; }
    private async Task NotificationClosed(Notification notification)
    {
        await UserNotificationService.DismissNotification(_currentUserId, notification.Id);
        notificationList.Remove(notification);
        StateHasChanged();
    }
    private async Task GetUserNotifications()
    {
        if (_currentUserId == null)
        {
            return;
        }

        notificationList = await UserNotificationService.GetActiveNotificationsForUser(_currentUserId, _currentRole);
    }
    private void HandleProjectStateChange()
    {
        InvokeAsync(StateHasChanged);
    }
    // quick filter - filter globally across multiple columns with the same input
    protected override async Task OnInitializedAsync()
    {
        LoadingService.Subscribe(isVisible =>
        {
            _loadingIndicator = isVisible;
            StateHasChanged();
        });
        ProjectState.OnChange += HandleProjectStateChange;
        _currentUserId = currentUser!.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!;
        _userName = currentUser.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value!;
        foreach (var column in _availableColumns)
        {
            _columnVisibility[column.Name] = column.isHidden;
        }
        if (currentUser.IsInRole("Admin"))
        {
            _currentRole = "Admin";
            _isAdminView = true;
            isAdminView = true;
            await GetColumnsPresetPreferences();

        }
        else if (currentUser.IsInRole("Editor"))
        {
            _currentRole = "Editor";
            _isEditorView = true;
            _isAdminView = false;

        }
        else if (currentUser.IsInRole("Client"))
        {
            _currentRole = "Client";
            _isClientView = true;
            _isAdminView = false;
        }
        CircuitId = Guid.NewGuid().ToString();
        Broadcaster.Subscribe(CircuitId, HandleProjectsUpdated);

    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            timezoneOffsetMinutes = await JSRuntime.InvokeAsync<int>("getTimezoneOffset");
            await GetUserNotifications();
            _loading = false;
            StateHasChanged();
        }

    }
    public void Dispose()
    {
        if (CircuitId != null)
        {
            Broadcaster.Unsubscribe(CircuitId);
        }
        ProjectState.OnChange -= HandleProjectStateChange;
    }

    private async Task ResetState()
    {
        _gridKey = Guid.NewGuid();
    }


    private async Task GetColumnsPresetPreferences()
    {
        try
        {
            _columnPresets = await columnPreferenceService.GetUserPresets(_currentUserId);
            
            _selectedPresetName = await columnPreferenceService.GetLastPresetUsed(_currentUserId);
            await LoadSelectedPreset(_selectedPresetName);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    private bool GetColumnVisibility(string columnName)
    {
        return !_columnVisibility.GetValueOrDefault(columnName, false);
    }

    private string _selectedPresetName;
    private MudMenu _menuRef;
    private async Task LoadSelectedPreset(string presetName)
    {
        if (string.IsNullOrEmpty(presetName))
        {

            _selectedPresetName = string.Empty;
            foreach (var column in _availableColumns)
            {
                _columnVisibility[column.Name] = column.isHidden;
            }
            Snackbar.Add("You've removed the filter", Severity.Info);
            return;
        }
        try
        {
            if (presetName == "createNewPreset")
            {
                await _menuRef.CloseAllMenusAsync();
                await OpenColumnPreferencesDialog();
                return;
            }
            // Get the preferences from the service
            var preferences = await columnPreferenceService.GetPreferencesByName(_currentUserId, presetName);

            // Update the visibility dictionary
            _columnVisibility = preferences;

            StateHasChanged();
            Snackbar.Add($"Preset '{presetName}' loaded successfully", Severity.Success);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"failed loading preset: {ex.Message}");
            Snackbar.Add($"Error loading preset. please contact support!", Severity.Error);
        }
    }

    // Open Dialog to Saves the visibility state as a new preset 
    // which can be loaded for fast access to the columns
    private async Task OpenColumnPreferencesDialog()
    {
        var parameters = new DialogParameters
        {
            ["VisibilityState"] = _columnVisibility,
            ["Columns"] = _availableColumns,
            ["_columnPresets"] = _columnPresets
        };

        var dialog = await DialogService.ShowAsync<ColumnPreferencesDialog>("Column Preferences", parameters);
        var result = await dialog.Result;

        if (!result!.Canceled)
        {
            if (result.Data is ColumnPreferenceResult presetResult)
            {
                _columnVisibility = presetResult.Visibility;
                await columnPreferenceService.SavePreset(_currentUserId, presetResult.PresetName, presetResult.Visibility);
                await GetColumnsPresetPreferences();
                _selectedPresetName = presetResult.PresetName;

            }
            else
            {
                _columnVisibility = (Dictionary<string, bool>) result.Data!;
            }
            StateHasChanged();
        }
    }
    private Task OnSearch(string text)
    {
        _searchString = text;
        return _dataGrid.ReloadServerData();
    }
  
    // Quick Global Filter which is used to search in the entire grid
    private Func<Project, bool> _quickFilter => x =>
    {
        if (string.IsNullOrWhiteSpace(_searchString))
            return true;

        // Null-safe string comparisons
        if (x.ProjectName?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
            return true;
        if (x.Description?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
            return true;

        // Combine all fields with null checks
        var searchableText = string.Join(" ", new string[]
        {
            x.Link,
            x.FormatStatus,
            x.FormatAdminStatus,
            x.ClientName,
            x.PrimaryEditorName,
            x.SecondaryEditorName,
            x.FormattedShootDate,
            x.FormattedDueDate,
            x.NotesForProject,
            x.FormattedWorkingMonth,
        }.Where(s => s != null));

        return searchableText.Contains(_searchString, StringComparison.OrdinalIgnoreCase);
    };


    private async Task OnClick()
    {
        await ScrollManager.ScrollToBottomAsync(".mud-table-container", ScrollBehavior.Smooth);
    }
    private bool _shouldScroll = false;


    private async Task OnRowClick(Project project)
    {
        StateHasChanged();
        await _dataGrid.SetEditingItemAsync(project);
        LoadingService.HideLoading();
        StateHasChanged();

    }
    private async Task HandleProjectsUpdated()
    {
        await InvokeAsync(async () =>
        {
            await LoadProjects();
            await _dataGrid.ReloadServerData();
            StateHasChanged();
        });
    }


    private async Task AdminToggle()
    {
        LoadingService.ShowLoading();
        _gridKey = Guid.NewGuid();
        _isAdminView = !_isAdminView;
        _isClientView = false;
        _isEditorView = false;
        _currentRole = "Admin";
        isSelectorVisible = true;
        await LoadProjects();
        await GetUserNotifications();
        LoadingService.HideLoading();
    }
    private async Task EditorToggle()
    {
        LoadingService.ShowLoading();
        _gridKey = Guid.NewGuid();
        _isEditorView = !_isEditorView;
        _isClientView = false;
        _isAdminView = false;
        _currentRole = "Editor";
        isSelectorVisible = false;
        await LoadProjects();
        await GetUserNotifications();

        LoadingService.HideLoading();
    }
    private async Task ClientToggle()
    {
        LoadingService.ShowLoading();
        _gridKey = Guid.NewGuid();
        _isClientView = !_isClientView;
        _isAdminView = false;
        _isEditorView = false;
        _currentRole = "Client";
        isSelectorVisible = false;
        await LoadProjects();
        await GetUserNotifications();
        LoadingService.HideLoading();
    }

    private async Task ArchivedToggle()
    {
        LoadingService.ShowLoading();
        _isArchived = !_isArchived;
        await LoadProjects();
        LoadingService.HideLoading();
    }
    private async Task OpenProjectChat(Project Project)
    {
        try
        {
            LoadingService.ShowLoading();
            await Task.Yield();
            ProjectState.SetProject(Project);
            await Task.Yield();
        }
        finally
        {
            LoadingService.HideLoading();
        }
    }
    /// <summary>
    /// Ensures thread-safe access to the LoadProjects method using SemaphoreSlim.
    /// SemaphoreSlim(1,1) creates a lightweight synchronization object that:
    /// - Allows only 1 thread to access the protected code at a time (first parameter)
    /// - Has a maximum count of 1 (second parameter)
    /// WaitAsync() acquires the lock before executing the code
    /// Release() ensures the lock is released after execution, even if an exception occurs
    /// This prevents race conditions when LoadProjects is called simultaneously by multiple events
    /// </summary>
    // Using it here to prevent LoadProject and RowsPerPageChanged from calling LoadProjects simultaneously
    private SemaphoreSlim _loadingSemaphore = new SemaphoreSlim(1, 1);
    //  Load project for admin/editor view only (will later be split )
    private async Task LoadProjects()
    {
        try
        {
            await _loadingSemaphore.WaitAsync();
            if (_isAdminView)
            {
                projects = await projectServices.GetProjectsAsync(_isArchived);
                Editors = await UserServices.GetEditorsWithProjectsAsync() ?? new List<UserRoleViewModel.UserProjectViewModel>();
                Clients = await UserServices.GetClientsWithProjectsAsync() ?? new List<UserRoleViewModel.UserProjectViewModel>();

            }
            // for client view which will fetch his own projects only)
            else if (_isClientView && isAdminView == false)
                await LoadProjectsForClient();
            // this is for the admin to see all the projects but from the client view
            else if (_isClientView && isAdminView == true || _isEditorView && isAdminView == true)
            {
                projects = await projectServices.GetProjectsAsync(_isArchived);
                Editors = await UserServices.GetEditorsWithProjectsAsync() ?? new List<UserRoleViewModel.UserProjectViewModel>();
                Clients = await UserServices.GetClientsWithProjectsAsync() ?? new List<UserRoleViewModel.UserProjectViewModel>();
            }
            else if (_isEditorView && isAdminView == false)
                await LoadProjectsForEditors();
            await _dataGrid.ReloadServerData();
            StateHasChanged();
            _shouldScroll = true;
        }
        finally
        {
            _loadingSemaphore.Release();

        }

    }
    // Loading Projects for Editors View
    private async Task LoadProjectsForEditors()
    {
        LoadingService.ShowLoading();
        projects = await projectServices.GetProjectsForEditors(_isArchived, _currentUserId);
        LoadingService.HideLoading();
        StateHasChanged();
    }
    // Loading Projects for Client View
    private async Task LoadProjectsForClient()
    {
        LoadingService.ShowLoading();
        projects = await projectServices.GetProjectsForClients(_isArchived, _currentUserId);
        LoadingService.HideLoading();
        StateHasChanged();
    }

    // Add new project dialog
    private async Task AddProjectDialogAsync(DialogOptions options)
    {
        LoadingService.ShowLoading();
        var currentUser = await UserServices.GetUserByIdAsync(_currentUserId);
        int weeksToDueDateDefault = currentUser.WeeksToDueDateDefault ?? 8;

        var dialogParameters = new DialogParameters
    {
        { "WeeksToDueDateDefault", weeksToDueDateDefault },
        { "_isClientView", _isClientView},
        { "Editors", Editors},
        { "Clients", Clients}
    };
        var dialog = await DialogService.ShowAsync<AddProjectDialog>("Add New Project", dialogParameters, options);
        var result = await dialog.Result;
        if (!result!.Canceled)
        {
            var newProject = (Project) result.Data!;
            if (newProject.ClientId == null)
            {
                newProject.ClientId = _currentUserId;
            }
            await projectServices.AddProjectAsync(newProject);
            await LoadProjects();
            StateHasChanged();
        }
        LoadingService.HideLoading();

    }
    HashSet<Project> SelectedProjects = new();

    private async Task SelectedItems(HashSet<Project> projects)
    {
        if (!_isAdminView)
            return;
        if (SelectedProjects == null || SelectedProjects.Count < 1)
        {
            Snackbar.Add("You didn't select any projects, please select projects first!", Severity.Error);
            return;
        }
        LoadingService.ShowLoading();
        try
        {
            var dialogParameters = new DialogParameters
        {
            { "SelectedProjects", SelectedProjects },
        };

            var dialog = await DialogService.ShowAsync<EditSelected>("Create New Project", dialogParameters);
            var result = await dialog.Result;

            if (!result!.Canceled)
            {
                var modifiedProjects = result.Data as HashSet<Project>;
                if (modifiedProjects != null)
                {
                    await projectServices.UpdateProjectsInBatchAsync(modifiedProjects, _currentUserId);
                    Snackbar.Add("Saved all projects successfully", Severity.Success);
                }
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add("Failed to update all projects. Please contact support.", Severity.Error);
            Console.WriteLine($"Error updating due date: {ex.Message}");
        }
        finally
        {
            LoadingService.HideLoading();
            SelectedProjects = new();
        }
    }
    private async Task CreateProjectDialogAsync(DialogOptions options)
    {
        LoadingService.ShowLoading();
        var currentUser = await UserServices.GetUserByIdAsync(_currentUserId);
        int weeksToDueDateDefault = currentUser.WeeksToDueDateDefault ?? 8;
        var dialogParameters = new DialogParameters
    {
        { "WeeksToDueDateDefault", weeksToDueDateDefault },
        { "_isClientView", _isClientView}
    };
        var dialog = await DialogService.ShowAsync<AddProjectDialog>("Create New Project", dialogParameters, options);
        LoadingService.HideLoading();
        var result = await dialog.Result;
        if (!result!.Canceled)
        {
            var newProject = (Project) result.Data!;
            newProject.WorkingMonth = DateTime.Today;
            newProject.ClientId = _currentUserId;
            await projectServices.AddProjectAsync(newProject);
            await LoadProjects();
            StateHasChanged();
            if (_isClientView)
            {
                var project = await projectServices.GetProjectByIdAsync(newProject.ProjectId);
                await _notificationService.QueueProjectCreationNotification(project);
            }
        }
    }
    // Deleting project in archive
    private async Task DeleteDialog(Project project)
    {
        projectToDelete = project;
        if (!await ConfirmationService.Confirm($"Are you sure you'd like to DELETE {project.ProjectName}? This will delete it permanently!"))
        {
            return;
        }
        LoadingService.ShowLoading();
        await projectServices.DeleteProjectAsync(projectToDelete.ProjectId);
        await LoadProjects();
        LoadingService.HideLoading();
        StateHasChanged();
    }
    // Archiving and Unarchiving the project
    public async Task ArchiveProject(Project project)
    {
        LoadingService.ShowLoading();
        var dialogParameters = new DialogParameters<ArchivingDialog> { { nameof(ArchivingDialog.newProject), project } };
        var dialog = await DialogService.ShowAsync<ArchivingDialog>("Archive Project", dialogParameters);
        var result = await dialog.Result;
        if (!result!.Canceled)
        {
            var archivedProject = (string) result.Data!;
            if (archivedProject != null)
            {
                await projectServices.ArchiveProjectAsync(project.ProjectId, archivedProject);
                await projectServices.ReorderProjectAsync(project.ProjectId, project.InternalOrder = null, false);
                await LoadProjects();

            }
        }
        LoadingService.HideLoading();

    }
    public async Task Duplicate(Project project)
    {
        try
        {
            LoadingService.ShowLoading();

            var newProject = new Project();
            newProject.ClientId = project.ClientId;
            newProject.ProjectName = project.ProjectName + "- Copy ";
            newProject.FootageLink = project.FootageLink;
            newProject.Deliverables = project.Deliverables;
            newProject.Description = project.Description;
            newProject.MusicPreference = project.MusicPreference;
            newProject.ShootDate = DateTime.UtcNow;
            newProject.DueDate = DateTime.UtcNow.AddDays(28);
            newProject.WorkingMonth = DateTime.UtcNow.Date;
            newProject.MusicPreference = project.MusicPreference;

            newProject.ProjectSpecifications = project.ProjectSpecifications;
            {
                newProject.ProjectSpecifications.ColorProfile = project.ProjectSpecifications.ColorProfile;
                newProject.ProjectSpecifications.CameraNumber = project.ProjectSpecifications.CameraNumber;
                newProject.ProjectSpecifications.Resolution = project.ProjectSpecifications.Resolution;
                newProject.ProjectSpecifications.Size = project.ProjectSpecifications.Size;
                newProject.ProjectSpecifications.AudioDetails = project.ProjectSpecifications.AudioDetails;
            };
            await projectServices.AddProjectAsync(newProject);
            await LoadProjects();
            LoadingService.HideLoading();
            Snackbar.Add("Successfully duplicated this project!", Severity.Success);


        }
        catch (Exception ex)
        {

            Snackbar.Add("Failed to duplicate this project !", Severity.Error);
            throw ex;

        }

    }

    private async Task UnarchiveProject(Project project)
    {
        LoadingService.ShowLoading();
        if (await ConfirmationService.Confirm($"Are you sure you'd like to unarchive this project {project.ProjectName}?"))
        {
            await projectServices.UnarchiveProjectAsync(project.ProjectId);
            await LoadProjects();
        }
        LoadingService.HideLoading();


    }
    // Opens Description Dialog
    private void OpenDescriptionDialog(Project context)
    {
        LoadingService.ShowLoading();
        var parameters = new DialogParameters
    {
            { "context", context },
            { "currentRole", _currentRole},
            { "modifiedProject", EventCallback.Factory.Create<Project>(this, async (modifiedProject) =>
                {
                    await updateProject(modifiedProject);
                })
            }
    };
        var options = new DialogOptions { };
        DialogService.ShowAsync<ProjectNameAndDescriptionDialog>("Project Description", parameters, options);
        LoadingService.HideLoading();

    }

    private async Task updateProject(Project modifiedProject)
    {
        LoadingService.ShowLoading();

        if (modifiedProject is Project project)
        {
            await UpdateProjectAsync(project);
        }
        LoadingService.HideLoading();

    }

    private void OpenViewClientEditingGuidelinesComponent(Project context)
    {
        LoadingService.ShowLoading();

        var parameters = new DialogParameters
    {
        { "userId", context.ClientId },
    };
        var options = new DialogOptions { FullScreen = false };
        DialogService.ShowAsync<ViewClientEditingGuidelinesComponent>("Client Preferences", parameters, options);
        LoadingService.HideLoading();

    }

    // Opens private note for modification
    private async Task OpensPrivateNoteForProject(Project context)
    {
        LoadingService.ShowLoading();

        var parameters = new DialogParameters
    {
        { "project", context },
    };
        var options = new DialogOptions { FullScreen = false };
        var dialog = await DialogService.ShowAsync<PrivateNoteForProjectComponent>("", parameters, options);
        var result = await dialog.Result;
        if (!result!.Canceled)
        {
            var project = (Project) result.Data!;
            await UpdateProjectAsync(project!);
            Snackbar.Add($"Successfully updated the project notes", Severity.Info);
            await LoadProjects();
        }
        LoadingService.HideLoading();

    }

    private async Task UpdateProjectAsync(Project project)
    {
        LoadingService.ShowLoading();
        await projectServices.UpdateProjectAsync(project, _currentUserId);
        LoadingService.HideLoading();

    }

    // Calculates Total Client Billable hours based on a formula.
    private async Task TimeCalculator(Project project)
    {
        LoadingService.ShowLoading();
        if (_isClientView)
            return;

        var dialogParameters = new DialogParameters<ProjectTotalHoursCalculatorDialog> {
        { nameof(ProjectTotalHoursCalculatorDialog.newProject), project },
        { "_isAdminView", _isAdminView},
        { "_isEditorView", _isEditorView}
    };
        var dialog = await DialogService.ShowAsync<ProjectTotalHoursCalculatorDialog>("", dialogParameters);
        var result = await dialog.Result;
        if (!result!.Canceled)
        {
            var _project = result.Data! as Project;
            if (_project != null)
            {
                await projectServices.UpdateProjectBillableHoursAsync(_project);
                await projectServices.CalculateProjectFinalPrice(_project);

                Snackbar.Add($"Successfully updated deliverable details of the project: {_project.ProjectName}", Severity.Success);
                await LoadProjects();
            }
        }
        LoadingService.HideLoading();

    }

    private async Task Logging(Project project, string currentUserId)
    {
        LoadingService.ShowLoading();

        var dialogParameters = new DialogParameters<LoggingDialog> {
            { nameof(LoggingDialog.currentProject), project },
            { "CurrentUserId", currentUserId},
            { "isAdmin", isAdminView}
        };

        var dialog = await DialogService.ShowAsync<LoggingDialog>("Logging Hours", dialogParameters);
        var result = await dialog.Result;
        await LoadProjects();
        LoadingService.HideLoading();


    }

    // Checks if there is changes between two projects in any property excluding the excluded.
    private bool HasNoSignificantChanges(Project original, Project modified, params string[] excludeProperties)
    {
        if (original == null || modified == null)
            return original == modified;  // Both null = true, one null = false

        var excluded = new HashSet<string>(excludeProperties, StringComparer.OrdinalIgnoreCase);  // Added case-insensitive comparison
        var properties = typeof(Project).GetProperties();

        foreach (var prop in properties)
        {
            if (excluded.Contains(prop.Name))
                continue;

            var originalValue = prop.GetValue(original);
            var modifiedValue = prop.GetValue(modified);

            if (!Equals(originalValue, modifiedValue))
                return false;
        }
        return true;
    }

    Project beforeModification = new Project();
    void StartedEditingItem(Project project)
    {
        beforeModification = JsonSerializer.Deserialize<Project>(
            JsonSerializer.Serialize(project))!;
    }
    void CanceledEditingItem(Project project)
    {

    }
    private async Task CommittedItemChanges(Project project)
    {
        try
        {
            LoadingService.ShowLoading();

            if (_isClientView)
            {
                if (HasNoSignificantChanges(beforeModification, project, "ExternalOrder", "Status", "ShootDate") == false)
                {
                    if (project.Status != ProjectStatus.Upcoming && (
                            string.IsNullOrEmpty(project.ProjectSpecifications?.Resolution) ||
                            string.IsNullOrEmpty(project.ProjectSpecifications?.Size) ||
                            string.IsNullOrEmpty(project.ProjectSpecifications?.CameraNumber) ||
                            string.IsNullOrEmpty(project.ProjectSpecifications?.ColorProfile) ))
                    {
                        throw new Exception("Please Open Project Brief and fill-out all the information before changing status.");
                    }
                    await UpdateProjectAsync(project);
                    Snackbar.Add($"Successfully updated the project", Severity.Info);
                    await LoadProjects();
                }
                else
                {
                    Snackbar.Add($"Detected unauthorized changes!", Severity.Error);
                }
            }
            else if (_isAdminView)
            {
                var editorId = beforeModification.PrimaryEditorId;
                if (editorId != project.PrimaryEditorId && editorId == null)
                {
                    bool isConfirm = await ConfirmationService.Confirm($"Do you want to change project status from {project.FormatStatus} to `Scheduled` status?");
                    if (isConfirm)
                    {
                        project.Status = ProjectStatus.Scheduled;
                    }
                }
                project = await confirmProjectStatusDelivered(project);
                await UpdateProjectAsync(project);
                Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;
                Snackbar.Add($"Successfully updated the project", Severity.Info);
                await LoadProjects();
            }
            else
            {
                if (HasNoSignificantChanges(beforeModification, project, "Status", "ShootDate", "DueDate", "ProgressBar", "WorkingMonth") == false)
                {
                    if (project.Status == ProjectStatus.Delivered && _isEditorView)
                    {
                        if (!await ConfirmationService.Confirm($"Make sure you have summitted the projects deliverables information in the calculator or if it's a non wedding project make sure to write it down to the private notes"))
                        {
                            Snackbar.Add("You have cancelled editing, changes won't be saved. ",Severity.Warning);
                            return;
                        }
                    }
                    project = await confirmProjectStatusDelivered(project);
                    await UpdateProjectAsync(project);
                    Snackbar.Add($"Successfully updated the project", Severity.Info);
                }
                else
                {
                    Snackbar.Add($"Detected unauthorized changes!", Severity.Error);
                }
            }

        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Warning);
            await LoadProjects();
        }
        finally
        {
            StateHasChanged();
            LoadingService.HideLoading();

        }
    }

    private string GetStatusStyle(ProjectStatus projectStatus)
    {
        return projectStatus switch
        {
            ProjectStatus.Upcoming => "background-color: #3f2e91; color: white; border-radius: 8px; --parent-bg-color:#7153ff; text-align:center;",
            ProjectStatus.Scheduled => "background-color: #320672; color: white; border-radius: 8px; --parent-bg-color: #7153ff; text-align:center;",
            ProjectStatus.Ready_To_Edit => "background-color: #0e8787; color: white; border-radius: 8px; --parent-bg-color: #00b2b2; text-align:center;",
            ProjectStatus.Working => "background-color: #b46f00; color: white; border-radius: 8px; --parent-bg-color: #fd9b00; text-align:center;",
            ProjectStatus.Delivered => "background-color: #002159; color: white; border-radius: 8px; --parent-bg-color: #2e77ff; text-align:center;",
            ProjectStatus.Ready_To_Review => "background-color: #6527a2; color: white; border-radius: 8px; --parent-bg-color: #b23eeb; text-align:center;",
            ProjectStatus.Revision => "background-color: #ae0502; color: white; border-radius: 8px; --parent-bg-color: #fd1c18; text-align:center;",
            ProjectStatus.Finished => "background-color: #003915; color: white; border-radius: 8px; --parent-bg-color: #008b33; text-align:center;",
            _ => "background-color: #757575; color: white; border-radius: 8px; --parent-bg-color: #616161; text-align:center;"
        };

    }
    private string GetAdminStatusStyle(AdminProjectStatus projectStatus)
    {
        return projectStatus switch
        {
            AdminProjectStatus.Not_Finished => "background-color: #3f2e91; color: white; border-radius: 8px; --parent-bg-color:#7153ff; text-align:center;",
            AdminProjectStatus.Sent_Invoice => "background-color: #b46f00; color: white; border-radius: 8px; --parent-bg-color: #fd9b00; text-align:center;",
            AdminProjectStatus.Delivered_Not_Paid => "background-color: #ae0502; color: white; border-radius: 8px; --parent-bg-color: #fd1c18; text-align:center;",
            AdminProjectStatus.Paid => "background-color: #003915; color: white; border-radius: 8px; --parent-bg-color: #008b33; text-align:center;",
        };

    }
    // Downloading Filtered as CSV for editors
    private async Task DownloadFilteredAsCsvEditors()
    {
        try
        {
            if (await ConfirmationService.Confirm("Do you want to download the filtered projects in CSV file?"))
            {
                var filteredItems = _dataGrid.FilteredItems.ToList();

                // Group projects by either PrimaryEditorName or SecondaryEditorName
                if (currentUser.IsInRole("Admin"))
                {
                    var groupedByEditor = filteredItems
                        .SelectMany(p => new[]
                        {
                        new { EditorName = p.PrimaryEditorName, Project = p },
                        new { EditorName = p.SecondaryEditorName, Project = p }
                        })
                        .GroupBy(e => e.EditorName)
                        .Where(g => !string.IsNullOrEmpty(g.Key) && g.Key != "N/A") // Filter out groups with empty editor names
                        .ToList();
                    foreach (var editorGroup in groupedByEditor)
                    {
                        var editorName = editorGroup.Key;
                        var projects = editorGroup.Select(e => e.Project).ToList();
                        var csvContent = GenerateCsvContentFilteredForEditors(projects, editorName);
                        var filename = $"{editorName.Replace(" ", "-")}_{DateTime.Now:MM_dd_yyyy}.csv";
                        await DownloadFile(filename, csvContent);
                    }
                }
                else
                {
                    var filteredProjects = filteredItems
                        .Where(p => p.PrimaryEditorId == _currentUserId || p.SecondaryEditorId == _currentUserId)
                        .ToList();
                    if (filteredProjects.Any())
                    {
                        var editorName = currentUser.Identity?.Name;
                        var csvContent = GenerateCsvContentFilteredForEditors(filteredProjects, editorName);
                        var filename = $"{editorName.Replace(" ", "-")}_{DateTime.Now:MM_dd_yyyy}.csv";
                        await DownloadFile(filename, csvContent);
                    }
                }
                   
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Download failed: {ex.Message}", Severity.Error);
        }
    }


    private async Task DownloadFilteredAsCsvPayoneer()
    {
        try
        {
            if (await ConfirmationService.Confirm("Do you want to download the filtered projects as Payoneer batch payment request?"))
            {
                    var filteredItems = _dataGrid.SelectedItems.ToList();
                    if(filteredItems.Count <= 0)
                    {
                        filteredItems = _dataGrid.FilteredItems.ToList();
                    }
                    var csvContent = await GenerateCsvContentFilteredPayoneer(filteredItems);
                    var filename = $"PayoneerPaymentBatch-{DateTime.Now:dd_MM_YYYY}.csv";
                    await DownloadFile(filename, csvContent);
                
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Download failed: {ex.Message}", Severity.Error);
        }
    }
    private async Task DownloadAllProjects()
    {
        try
        {
            if (await ConfirmationService.Confirm("Do you want to download the All Projects as CSV file?"))
            {
                if (projects.Any())
                {
                    var nonNullProjects = projects.Where(p => p != null).Cast<Project>().ToList(); // Remove null warning
                    var csvContent = ConvertProjectsToCsv(nonNullProjects);
                    var filename = $"All_Projects_{DateTime.Now:dd_MM_yyyy_HH_mm_ss}.csv";
                    await DownloadFile(filename, csvContent);
                }
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Download failed: {ex.Message}", Severity.Error);
        }
    }
    private string FormatStringToCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        // If the string contains commas, newline characters, or double quotes, wrap it in double quotes
        if (value.Contains(",") || value.Contains("\n") || value.Contains("\r") || value.Contains("\""))
        {
            // Escape double quotes by replacing them with two double quotes
            value = "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        return value;
    }

    public string ConvertProjectsToCsv(List<Project> projects)
    {
        var sb = new System.Text.StringBuilder();

        // Add CSV headers with the additional fields for editor details
        sb.AppendLine("ID,Project Name,Project Description,Client Name,Primary Editor Name,Secondary Editor Name,Progress,Shoot Date,Due Date,Status, Payment Status,Client Billable Hours,Client Billable Amount,Primary Editor Billable Hours,Primary Editor Overtime,Primary Editor Payment Amount,Secondary Editor Billable Hours,Secondary Editor Overtime,Secondary Editor Payment Amount, Revisions");

        // Add each project as a CSV line
        foreach (var project in projects)
        {
            // Populate the names for the client and editors
            var clientName = project.Client?.UserName ?? "N/A";
            var primaryEditorName = project.PrimaryEditor?.UserName ?? "N/A";
            var secondaryEditorName = project.SecondaryEditor?.UserName ?? "N/A";
            // Remove HTML tags from the Description
            var descriptionWithoutHtml = project.Description != null ? System.Text.RegularExpressions.Regex.Replace(project.Description, "<.*?>", String.Empty) : String.Empty; // Removes HTML tags
            descriptionWithoutHtml = FormatStringToCsv(descriptionWithoutHtml);

            // Retrieve the Billable Hours and Payment Amounts for Client and Editors
            var clientBillableHours = project.ClientBillableHours?.ToString("0.##") ?? "0";
            var clientBillableAmount = project.ClientBillableAmount?.ToString("0.##") ?? "0";

            var primaryEditorBillableHours = project.PrimaryEditorDetails?.BillableHours?.ToString("0.##") ?? "0";
            var primaryEditorOvertime = project.PrimaryEditorDetails?.Overtime?.ToString("0.##") ?? "0";
            var primaryEditorPaymentAmount = project.PrimaryEditorDetails?.PaymentAmount?.ToString("0.##") ?? "0";

            var secondaryEditorBillableHours = project.SecondaryEditorDetails?.BillableHours?.ToString("0.##") ?? "0";
            var secondaryEditorOvertime = project.SecondaryEditorDetails?.Overtime?.ToString("0.##") ?? "0";
            var secondaryEditorPaymentAmount = project.SecondaryEditorDetails?.PaymentAmount?.ToString("0.##") ?? "0";
            var revisions = project.Revisions != null && project.Revisions.Any()
            ? project.Revisions.Select(r =>
                $"{r.RevisionDate.ToString("MM-dd-yyyy")}: {r.Content.Replace("\n", " ").Replace("\r", "")}")
                .Aggregate((current, next) => current + " || " + next)
               : string.Empty;
            revisions = FormatStringToCsv(revisions);
            // Append the project data to the CSV
            sb.AppendLine($"{project.ProjectId},{project.ProjectName},{descriptionWithoutHtml},{clientName},{primaryEditorName},{secondaryEditorName},{project.ProgressBar},{project.FormattedShootDate},{project.FormattedDueDate},{project.FormatStatus},{project.FormatAdminStatus},{clientBillableHours},{clientBillableAmount},{primaryEditorBillableHours},{primaryEditorOvertime},{primaryEditorPaymentAmount},{secondaryEditorBillableHours},{secondaryEditorOvertime},{secondaryEditorPaymentAmount},{revisions}");
        }

        return sb.ToString();
    }


    private async Task DownloadFile(string filename, string content)
    {
        try
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            await JSRuntime.InvokeVoidAsync(
                "saveAsFile",
                filename,
                Convert.ToBase64String(bytes)
            );
            Snackbar.Add($"File has started downloading!", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"File save error: {ex.Message}", Severity.Error);
        }
    }
    private string GenerateCsvContentFilteredForEditors(List<Project> projects, string editorName)
    {
        // Create CSV header
        decimal total = 0;
        var csv = new System.Text.StringBuilder();
        csv.AppendLine(string.Join(",",
            nameof(Project.ProjectName),
            "Editor Name",
            "Payment Amount"
        ));

        // Add data rows
        foreach (var project in projects)
        {
            decimal paymentAmount = 0;

            // Check if the editor is Primary or Secondary and assign the PaymentAmount
            if (project.PrimaryEditorName == editorName)
            {
                paymentAmount = project.PrimaryEditorDetails?.PaymentAmount ?? 0;
            }
            else if (project.SecondaryEditorName == editorName)
            {
                paymentAmount = project.SecondaryEditorDetails?.PaymentAmount ?? 0;
            }

            csv.AppendLine(string.Join(",",
                EscapeCsvValue(project.ProjectName),
                EscapeCsvValue(editorName),
                $"${Math.Round(paymentAmount, MidpointRounding.AwayFromZero)}"
            ));

            total += paymentAmount;
        }

        // Add total
        csv.AppendLine();
        csv.AppendLine(string.Join(",",
            $"Total Payment: ${Math.Round(total, MidpointRounding.AwayFromZero)}",
            $"  Date: {DateTime.Now:dd-MM-yyyy}"
        ));

        return csv.ToString();
    }



    [Inject] private PayoneerSettingsService payoneerSettingsService { get; set; }
    private async Task<string> GenerateCsvContentFilteredPayoneer(List<Project> projects)
    {
        // Get unique client IDs from projects
        List<string> clientIds = projects.Select(p => p.ClientId).Distinct().ToList();

        // Get Payoneer settings for all clients
        Dictionary<string, PayoneerSettings> clientSettings = await payoneerSettingsService.GetSettingsForClientsAsync(clientIds);

        IEnumerable<IGrouping<string, Project>> groupedByClient = projects.GroupBy(p => p.ClientId);
        var csv = new System.Text.StringBuilder();

        // Payoneer CSV headers
        csv.AppendLine(string.Join(",",
            "Company Name",
            "Company URL",
            "First Name",
            "Last Name",
            "Email",
            "Amount",
            "Currency",
            "Description",
            "Payment Due By"));

        foreach (var client in groupedByClient)
        {
            var clientName = client.First().ClientName;
            if (!clientSettings.TryGetValue(client.Key, out var settings))
            {
                // Skip if no settings found for this client
                Snackbar.Add($"User {clientName} does not have Payoneer settings. The CSV won't include his projects!! ", Severity.Warning);
                continue;
            }

            decimal clientTotal = client.Sum(p => p.ClientBillableAmount ?? 0);
            var projectDescription = GenerateProjectDescription(client.ToList(),settings.Currency);
            projectDescription += $"   Total: ${Math.Round(clientTotal, MidpointRounding.AwayFromZero)}";
            csv.AppendLine(string.Join(",",
                EscapeCsvValue(settings.CompanyName),
                EscapeCsvValue(settings.CompanyUrl ?? ""),
                EscapeCsvValue(settings.FirstName),
                EscapeCsvValue(settings.LastName),
                EscapeCsvValue(settings.Email),
                $"{Math.Round(clientTotal, MidpointRounding.AwayFromZero)}",
                settings.Currency,
                EscapeCsvValueDescritpion(projectDescription),
                DateTime.Now.AddDays(7).ToString("dd-MM-yyyy")
            ));
        }
        return csv.ToString();
    }
    // This generates the description for the Payoneer CSV by combining the projects and returning the total amount.
    private string GenerateProjectDescription(List<Project> projects,string currency)
    {
        var projectDescriptions = projects.Select(p => $"{p.ProjectName}: {Math.Round(p.ClientBillableAmount ?? 0, MidpointRounding.AwayFromZero)}").ToList();
        return string.Join(" ", projectDescriptions);
    }
    private string EscapeCsvValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        // Escape commas and quotes
        value = value.Replace("\"", "\"\"");
        if (value.Contains(",") || value.Contains("\"") || value.Contains("/") || value.Contains("\n"))
        {
            value = $"\"{value}\"";
        }
        return value;
    }
    private string EscapeCsvValueDescritpion(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        // Replace / and \ with -
        value = value.Replace("/", "-").Replace("\\", "-");

        // Escape double quotes
        value = value.Replace("\"", "\"\"");

        // Wrap in quotes if it contains special CSV characters
        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
        {
            value = $"\"{value}\"";
        }
        return value;
    }

    // DRAGGING FUNCTIONALITIES 
    private List<Project> _Projects { get; set; } = new();
    private Project? draggedProject;
    private Project? dropTarget;
    private bool isDragging;



    private string GetRowClass(Project project, int rowNumber)
    {
        var classes = new List<string> { };

        if (isDragging && dropTarget?.ProjectId == project.ProjectId)
            classes.Add("drop-target");

        if (draggedProject?.ProjectId == project.ProjectId)
            classes.Add("being-dragged");

        return string.Join(" ", classes);
    }

    private void HandleDragStart(Project project)
    {
        if (project == null)
            return;
        draggedProject = project;
        isDragging = true;
        StateHasChanged();
    }

    private void HandleDragOver(Project target)
    {
        if (draggedProject == null || target == null)
            return;
        if (draggedProject?.ProjectId == target.ProjectId)
            return;
    }

    private void HandleDragEnter(Project target)
    {
        if (draggedProject == null || target == null)
            return;
        if (draggedProject?.ProjectId != target.ProjectId)
        {
            dropTarget = target;
            StateHasChanged();
        }
    }

    private void HandleDragLeave(Project target)
    {
        if (draggedProject == null || target == null)
            return;
        if (target != null && dropTarget?.ProjectId == target.ProjectId)
        {
            dropTarget = null;
            StateHasChanged();
        }
    }

    private async Task HandleDropForInternalOrder(Project targetProject)
    {
        try
        {
            LoadingService.ShowLoading();

            if (draggedProject == null || targetProject == null)
                return;

            if (draggedProject.ProjectId == targetProject.ProjectId)
                return;

            int? newOrder = targetProject.InternalOrder;
            RecordSwap(draggedProject.ProjectId,draggedProject.ProjectName, targetProject.ProjectName, draggedProject.InternalOrder!.Value , targetProject.InternalOrder!.Value);
            UpdateUndoRedoTexts();
            await projectServices.ReorderProjectAsync(draggedProject.ProjectId, newOrder, false);

            Snackbar.Add($"Project: {draggedProject.ProjectName} has been moved to '{newOrder}' successfully", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add("Failed to update project order", Severity.Error);
            Console.WriteLine(ex);
        }
        finally
        {
            // Reset state
            isDragging = false;
            draggedProject = null;
            dropTarget = null;
            LoadingService.HideLoading();
            ResetDragState();
            await RefreshGridData();
        }
    }
    private async Task HandleDropForExternalOrder(Project targetProject)
    {
        try
        {
            LoadingService.ShowLoading();

            if (draggedProject == null || targetProject == null)
                return;

            if (draggedProject.ProjectId == targetProject.ProjectId)
                return;

            int? newOrder = targetProject.ExternalOrder;
            await projectServices.ReorderProjectAsync(draggedProject.ProjectId, newOrder, true);

            Snackbar.Add($"Project: {draggedProject.ProjectName} has been moved to '{newOrder}' successfully", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add("Failed to update project order", Severity.Error);
            Console.WriteLine(ex);
        }
        finally
        {
            // Reset state
            isDragging = false;
            draggedProject = null;
            dropTarget = null;
            LoadingService.HideLoading();
            ResetDragState();
            await RefreshGridData();
        }
    }
    private void ResetDragState()
    {
        isDragging = false;
        draggedProject = null;
        dropTarget = null;
    }
    private async Task RefreshGridData()
    {
        projects = ( await projectServices.GetProjectsAsync() ).ToList();
        await LoadProjects();
        StateHasChanged();
    }

    private async Task ToggleUrgentVisibility(Project project)
    {
        try
        {
            LoadingService.ShowLoading();

            var newValue = !project.IsUrgent;
            project.IsUrgent = newValue;
            await UpdateProjectAsync(project);
            if (newValue == true)
            { Snackbar.Add($"Warning is visible now.", Severity.Success); }
            else
            {
                Snackbar.Add($"Warning is hidden now.", Severity.Info);
            }
            LoadingService.HideLoading();

        }
        catch (Exception ex)
        {
            Snackbar.Add("Failed to add warning!", Severity.Error);
            Console.WriteLine($"Error updating warning urgent visibility: {ex.Message}");
        }
    }
    private async Task TogglePaymentVisibility(Project project)
    {
        try
        {
            LoadingService.ShowLoading();

            var newValue = !project.IsPaymentVisible;
            project.IsPaymentVisible = newValue;
            await UpdateProjectAsync(project);
            if (newValue == true)
            { Snackbar.Add($"Payment is visible to the client now.", Severity.Success); }
            else
            {
                Snackbar.Add($"Payment is hidden from the client now.", Severity.Info);
            }
            LoadingService.HideLoading();

        }
        catch (Exception ex)
        {
            Snackbar.Add("Failed to update payment visibility", Severity.Error);
            Console.WriteLine($"Error updating payment visibility: {ex.Message}");
        }
    }
    private async Task OpenClientNameDialog(Project project)
    {
        try
        {
            if (_isClientView)
                return;

            var parameters = new DialogParameters
        {
            { "_project", project },
            { "Clients", Clients }
        };

            var dialog = await DialogService.ShowAsync<ClientNameDialog>("", parameters);
            var result = await dialog.Result;

            if (!result!.Canceled)
            {
                var _project = (Project) result.Data!;
                await UpdateProjectAsync(_project);
                Snackbar.Add("Saved Successfully", Severity.Success);

            }

        }
        catch (Exception ex)
        {
            Snackbar.Add("Failed to update progress", Severity.Error);
            Console.WriteLine($"Error updating progress: {ex.Message}");
        }
    }
    private async Task OpenLinkDialog(Project project)
    {
        try
        {
            if (_isClientView)
            {
                return;
            }

            var parameters = new DialogParameters
        {
            { "project", project },
        };

            var dialog = await DialogService.ShowAsync<ProjectLink>("", parameters);
            var result = await dialog.Result;

            if (!result!.Canceled)
            {
                var _project = (Project) result.Data!;
                await UpdateProjectAsync(_project);
                Snackbar.Add("Saved Successfully", Severity.Success);

            }

        }
        catch (Exception ex)
        {
            Snackbar.Add("Failed to update progress", Severity.Error);
            Console.WriteLine($"Error updating progress: {ex.Message}");
        }
    }
    private async Task OpenProgressDialog(Project project)
    {
        try
        {
            if (_isClientView)
                return;

            var parameters = new DialogParameters
        {
            { "Progress", project.ProgressBar }
        };

            var dialog = await DialogService.ShowAsync<ProgressEditDialog>("", parameters);
            var result = await dialog.Result;

            if (!result!.Canceled)
            {
                project.ProgressBar = (int) result.Data!;
                await UpdateProjectAsync(project);
                Snackbar.Add("Saved Successfully", Severity.Success);

            }

        }
        catch (Exception ex)
        {
            Snackbar.Add("Failed to update progress", Severity.Error);
            Console.WriteLine($"Error updating progress: {ex.Message}");
        }
    }
    private async Task OpenShootDateDialog(Project project)
    {
        try
        {
            var parameters = new DialogParameters
        {
            { "ShootDate", project.ShootDate }
        };

            var dialog = await DialogService.ShowAsync<ShootDateEditDialog>("", parameters);
            var result = await dialog.Result;

            if (!result!.Canceled)
            {
                project.ShootDate = (DateTime) result.Data!;
                await UpdateProjectAsync(project);
                Snackbar.Add("Saved Successfully", Severity.Success);

            }
        }
        catch (Exception ex)
        {
            Snackbar.Add("Failed to update shoot date", Severity.Error);
            Console.WriteLine($"Error updating shoot date: {ex.Message}");
        }
    }

    private async Task OpenDueDateDialog(Project project)
    {
        try
        {
            if (!_isAdminView)
                return;

            var parameters = new DialogParameters
        {
            { "DueDate", project.DueDate },
            { "IsUrgent", project.IsUrgent }
        };

            var dialog = await DialogService.ShowAsync<DueDateDialog>("", parameters);
            var result = await dialog.Result;

            if (!result!.Canceled)
            {
                (project.DueDate, project.IsUrgent) = ((DateTime, bool)) result.Data!;
                await UpdateProjectAsync(project);
                Snackbar.Add("Saved Successfully", Severity.Success);

            }

        }
        catch (Exception ex)
        {
            Snackbar.Add("Failed to update due date", Severity.Error);
            Console.WriteLine($"Error updating due date: {ex.Message}");
        }
    }
    private async Task OpenWorkingMonthDialog(Project project)
    {
        try
        {
            if (!_isAdminView)
                return;

            var parameters = new DialogParameters
        {
            { "WorkingMonth", project.WorkingMonth }
        };

            var dialog = await DialogService.ShowAsync<WorkingMonthDialog>("", parameters);
            var result = await dialog.Result;

            if (!result!.Canceled)
            {
                project.WorkingMonth = (DateTime) result.Data!;
                await UpdateProjectAsync(project);
                Snackbar.Add("Saved Successfully", Severity.Success);

            }

        }
        catch (Exception ex)
        {
            Snackbar.Add("Failed to update working month", Severity.Error);
            Console.WriteLine($"Error updating working month: {ex.Message}");
        }
    }
    private async Task OpenStatusDialog(Project project)
    {
        try
        {
            var parameters = new DialogParameters
            {
                { "Status", project.Status },
                { "_isAdminView",_isAdminView },
                { "_isEditorView", _isEditorView },
                { "_isClientView", _isClientView }

            };

            var dialog = await DialogService.ShowAsync<StatusDialog>("", parameters);
            var result = await dialog.Result;

            if (!result!.Canceled)
            {
                project.Status = (ProjectStatus) result.Data!;
                if (project.Status == ProjectStatus.Delivered && _isEditorView)
                {
                    if (!await ConfirmationService.Confirm($"Make sure you have summitted the projects deliverables information in the calculator or if it's a non wedding project make sure to write it down to the private notes"))
                    {
                        return;
                    }
                }
                project = await confirmProjectStatusDelivered(project);
                await UpdateProjectAsync(project);
                Snackbar.Add("Saved Successfully", Severity.Success);

            }


        }
        catch (Exception ex)
        {
            Snackbar.Add("Failed to update status", Severity.Error);
            Console.WriteLine($"Error updating status: {ex.Message}");
        }
    }
    private async Task<Project> confirmProjectStatusDelivered(Project project)
    {
        if (project.Status == ProjectStatus.Delivered)
        {
            bool isConfirm = await ConfirmationService.Confirm($"Do you want to change payment status from {project.FormatAdminStatus} to `Delivered Not Paid` status?");
            if (isConfirm)
            {
                project.AdminStatus = AdminProjectStatus.Delivered_Not_Paid;
            }
        }
        return project;
    }
    private async Task OpenAdminStatusDialog(Project project)
    {
        try
        {
            var parameters = new DialogParameters
       {
           { "AdminStatus", project.AdminStatus }
       };

            var dialog = await DialogService.ShowAsync<AdminStatusDialog>("", parameters);
            var result = await dialog.Result;

            if (!result!.Canceled)
            {
                project.AdminStatus = (AdminProjectStatus) result.Data!;
                await UpdateProjectAsync(project);
                Snackbar.Add("Saved Successfully", Severity.Success);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add("Failed to update payment status", Severity.Error);
            Console.WriteLine($"Error updating payment status: {ex.Message}");
        }
    }
    private async Task OpenPrimaryEditorDialog(Project project)
    {
        try
        {
            var parameters = new DialogParameters
        {
            { "PrimaryEditorId", project.PrimaryEditorId },
            { "Editors", Editors }

        };

            var dialog = await DialogService.ShowAsync<PrimaryEditorDialog>("", parameters);
            var result = await dialog.Result;

            if (!result!.Canceled)
            {
                var editorId = project.PrimaryEditorId;
                project.PrimaryEditorId = (string) result.Data!;
                if(editorId != project.PrimaryEditorId && editorId == null)
                {
                    bool isConfirm = await ConfirmationService.Confirm($"Do you want to change project status from {project.FormatStatus} to `Scheduled` status?");
                    if (isConfirm)
                    {
                        project.Status = ProjectStatus.Scheduled;
                    }
                }
                await UpdateProjectAsync(project);
                Snackbar.Add("Saved Successfully", Severity.Success);

            }
        }
        catch (Exception ex)
        {
            Snackbar.Add("Failed to update primary editor", Severity.Error);
            Console.WriteLine($"Error updating primary editor: {ex.Message}");
        }
    }
    private async Task OpenSecondaryEditorDialog(Project project)
    {
        try
        {
            var parameters = new DialogParameters
        {
            { "SecondaryEditorId", project.SecondaryEditorId },
            { "Editors", Editors }

        };

            var dialog = await DialogService.ShowAsync<SecondaryEditorDialog>("", parameters);
            var result = await dialog.Result;

            if (!result!.Canceled)
            {
                project.SecondaryEditorId = (string) result.Data!;
                await UpdateProjectAsync(project);
                Snackbar.Add("Saved Successfully", Severity.Success);

            }
        }
        catch (Exception ex)
        {
            Snackbar.Add("Failed to update secondary editor", Severity.Error);
            Console.WriteLine($"Error updating secondary editor: {ex.Message}");
        }
    }
    private async Task OpenSecondaryEditorPaymentDate(Project project)
    {
        try
        {
            var parameters = new DialogParameters
            {
                { "_SecondaryEditorPaymentDate", project.SecondaryEditorDetails.DatePaidEditor }

            };

            var dialog = await DialogService.ShowAsync<SecondaryEditorPaymentDate>("", parameters);
            var result = await dialog.Result;

            if (!result!.Canceled)
            {
                project.SecondaryEditorDetails.DatePaidEditor = (DateTime?) result.Data!;
                await UpdateProjectAsync(project);
                Snackbar.Add("Saved Successfully", Severity.Success);

            }
        }
        catch (Exception ex)
        {
            Snackbar.Add("Failed to update secondary editor payment date", Severity.Error);
            Console.WriteLine($"Error updating secondary editor payment date: {ex.Message}");
        }
    }
    private async Task OpenPrimaryEditorPaymentDate(Project project)
    {
        try
        {
            var parameters = new DialogParameters
        {
            { "_PrimaryEditorPaymentDate", project.PrimaryEditorDetails.DatePaidEditor }
        };

            var dialog = await DialogService.ShowAsync<PrimaryEditorPaymentDate>("", parameters);
            var result = await dialog.Result;

            if (!result!.Canceled)
            {
                project.PrimaryEditorDetails.DatePaidEditor = (DateTime?) result.Data!;
                await UpdateProjectAsync(project);
                Snackbar.Add("Saved Successfully", Severity.Success);

            }
        }
        catch (Exception ex)
        {
            Snackbar.Add("Failed to update Primary editor payment date", Severity.Error);
            Console.WriteLine($"Error updating Primary editor payment date: {ex.Message}");
        }
    }
    [Inject] IJSRuntime JS { get; set; }
    private async Task CopyProjectNameToClipboard(Project project)
    {
        var nameToCopy = $"{project.FormattedShootDate}-{project.ProjectName}";
        await JS.InvokeVoidAsync("copyToClipboard", nameToCopy);
    }
    [Inject] UndoRedoService UndoRedoService { get; set; }
    string undoSwapText;
    string redoSwapText;
    private void UpdateUndoRedoTexts()
    {
        var undoAction = UndoRedoService.UndoText();
        var redoAction = UndoRedoService.RedoText();

        undoSwapText = undoAction != null
            ? $"Swap `{undoAction.ProjectName}` with `{undoAction.TargetName}` where `{undoAction.ProjectName}` order will be {undoAction.SourceReorderId}"
            : string.Empty;

        redoSwapText = redoAction != null
            ? $"Swap `{redoAction.ProjectName}` with `{redoAction.TargetName}` where `{redoAction.ProjectName}` order will be {redoAction.TargetReorderId}"
            : string.Empty;
    }
    private async Task Undo()
    {
        try
        {
            var action = UndoRedoService.Undo();
            if (action != null)
            {
                await projectServices.ReorderProjectAsync(action.ProjectId,action.SourceReorderId, false);
            }
            UpdateUndoRedoTexts();
        }
        catch(Exception ex)
        {
            Snackbar.Add("Failed to undo", Severity.Error);
            Console.WriteLine($"Error undoing: {ex.Message}");
        }

    }
    private async Task Redo()
    {
        try
        {
            var action = UndoRedoService.Redo();
            UpdateUndoRedoTexts();

            if (action != null)
            {
                await projectServices.ReorderProjectAsync(action.ProjectId,action.TargetReorderId, false);

            }
        }
        catch (Exception ex)
        {
            Snackbar.Add("Failed to redo", Severity.Error);
            Console.WriteLine($"Error undoing: {ex.Message}");
        }

    }
    
    private void RecordSwap(int projectId,string projectName, string targetName, int sourceId, int targetId)
    {
        UndoRedoService.AddSwap(projectId, projectName, targetName, sourceId, targetId);
    }
    private async Task<GridData<Project>> ServerDataFunc(GridStateVirtualize<Project> gridState, CancellationToken token)
    {
        try
        {
            var result = projects;
            await Task.Delay(25);
            if(projects.Count == 0 )
            {
                await LoadProjects();
                Console.WriteLine("Called ServerDataFunc  again");

            }
            if (result.Count == 0 || result.Count != projects.Count)
            {
                result = projects;
            }

            result = result.Where(project =>
            {
                if (string.IsNullOrWhiteSpace(_searchString))
                    return true;

                if (project.ProjectName?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
                    return true;
                if (project.Description?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
                    return true;

                if (( $"{project.Link ?? ""} {project.FormatStatus ?? ""} {project.FormatAdminStatus ?? ""} {project.ClientName ?? ""} " +
                     $"{project.PrimaryEditorName ?? ""} {project.SecondaryEditorName ?? ""} {project.FormattedShootDate ?? ""} " +
                     $"{project.FormattedDueDate ?? ""} {project.NotesForProject ?? ""} {project.FormattedWorkingMonth ?? ""}" )
                    .Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                    return true;

                return false;
            }).ToList();
            if (gridState.SortDefinitions.Count > 0)
            {
                var firstSort = gridState.SortDefinitions.First();
                result = firstSort.Descending
                    ? result.OrderByDescending(firstSort.SortFunc).ToList()
                    : result.OrderBy(firstSort.SortFunc).ToList();
            }

            if (gridState.FilterDefinitions.Any())
            {
                var filterFunctions = gridState.FilterDefinitions.Select(x => x.GenerateFilterFunction());
                result = result
                    .Where(x => filterFunctions.All(f => f(x)))
                    .ToList();
            }

            var totalNumberOfFilteredItems = result.Count;

            result = result
                .Skip(gridState.StartIndex)
                .Take(gridState.Count)
                .ToList();


            return new GridData<Project>
            {
                Items = result,
                TotalItems = totalNumberOfFilteredItems
            };
        }
        catch (TaskCanceledException)
        {
            return new GridData<Project>
            {
                Items = [],
                TotalItems = 0
            };
        }
    }
}
