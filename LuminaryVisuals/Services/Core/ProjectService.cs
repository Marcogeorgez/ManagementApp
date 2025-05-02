namespace LuminaryVisuals.Services.Core;

using LuminaryVisuals.Data;
using LuminaryVisuals.Data.Entities;
using LuminaryVisuals.Models;
using LuminaryVisuals.Services.Events;
using LuminaryVisuals.Services.Mail;
using LuminaryVisuals.Services.Shared;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using Npgsql;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static LuminaryVisuals.Components.Pages.CalenderPage;

public class ProjectService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ILogger<ProjectService> _logger;
    private readonly CircuitUpdateBroadcaster _broadcaster;
    private readonly INotificationService _notificationService;
    private readonly LoggingHours loggingHours;
    private readonly ChatService chatService;
    private bool IsConcurrencyConflict(DbUpdateException ex) => ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505";

        // Check if the exception is due to a concurrency conflict (e.g., duplicate key)
    
    public ProjectService(IDbContextFactory<ApplicationDbContext> context, ILogger<ProjectService> logger, ChatService chatService,
        CircuitUpdateBroadcaster projectUpdateService, INotificationService notificationService, LoggingHours loggingHours)
    {
        _contextFactory = context;
        _logger = logger;
        _broadcaster = projectUpdateService;
        _notificationService = notificationService;
        this.chatService = chatService;
        this.loggingHours = loggingHours;
    }

    public async Task<List<Project>> GetProjectsAsync(bool isArchived)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var projects = await context.Projects
                .AsTracking()
                .Where(p => p.IsArchived == isArchived)
                .Include(p => p.Archive)
                .Include(p => p.Client)
                .Include(p => p.PrimaryEditor)
                .Include(p => p.SecondaryEditor)
                .Include(p => p.Revisions)
                .OrderBy(p => p.InternalOrder)
                .ToListAsync();


            var userIds = projects
                .SelectMany(p => new[] { p.ClientId, p.PrimaryEditorId, p.SecondaryEditorId })
                .Where(id => id != null)
                .Distinct()
                .ToList();

            var userNames = await context.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName, u.HourlyRate })
                .ToDictionaryAsync(u => u.Id, u => u.UserName);

            foreach (var project in projects)
            {
                project.ClientName = project.ClientId != null && userNames.ContainsKey(project.ClientId)
                    ? userNames[project.ClientId]!
                    : "N/A ??";
                project.PrimaryEditorName = project.PrimaryEditorId != null && userNames.ContainsKey(project.PrimaryEditorId)
                    ? userNames[project.PrimaryEditorId]!
                    : "N/A";
                project.SecondaryEditorName = project.SecondaryEditorId != null && userNames.ContainsKey(project.SecondaryEditorId)
                    ? userNames[project.SecondaryEditorId]!
                    : "N/A";
            }

            return projects;
        }
    }
    public async Task<List<Project>> GetProjectsDateRangeCalenderAsync(bool isArchived, DateRange dateRange, bool isAdmin,string userId)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var projects = await context.Projects
                .Where(p => p.IsArchived == isArchived &&
                    (
                        ( dateRange.Start == null || p.ShootDate >= dateRange.Start ) &&
                        ( dateRange.End == null || p.ShootDate <= dateRange.End ) ||
                        ( dateRange.Start == null || p.DueDate >= dateRange.Start ) &&
                        ( dateRange.End == null || p.DueDate <= dateRange.End ) ||
                        ( dateRange.Start == null || p.StartDate >= dateRange.Start ) &&
                        ( dateRange.End == null || p.StartDate <= dateRange.End ) ||
                        ( dateRange.Start == null || p.EndDate >= dateRange.Start ) &&
                        ( dateRange.End == null || p.EndDate <= dateRange.End )
                    )
                )
                .Include(p => p.Client)
                .Include(p => p.PrimaryEditor)
                .Include(p => p.SecondaryEditor)
                .ToListAsync();
            if(!isAdmin)
            {
                projects = projects.Where(p => p.PrimaryEditor?.Id == userId || p.SecondaryEditor?.Id == userId).ToList();
            }

            return projects;
        }
    }
    public async Task<Project?> GetProjectByIdAsync(int projectId)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var project = await context.Projects
                .AsTracking()
                .Where(p => p.ProjectId == projectId)
                .Include(p => p.Archive)
                .Include(p => p.Client)
                .Include(p => p.PrimaryEditor)
                .Include(p => p.SecondaryEditor)
                .Include(p => p.Revisions)
                .FirstOrDefaultAsync();

            if (project != null)
            {
                var userIds = new[] { project.ClientId, project.PrimaryEditorId, project.SecondaryEditorId }
                    .Where(id => id != null)
                    .Distinct()
                    .ToList();

                var userNames = await context.Users
                    .Where(u => userIds.Contains(u.Id))
                    .Select(u => new { u.Id, u.UserName })
                    .ToDictionaryAsync(u => u.Id, u => u.UserName);

                project.ClientName = userNames.GetValueOrDefault(project.ClientId, "No Client Assigned ??");
                project.PrimaryEditorName = project.PrimaryEditorId != null
                    ? userNames.GetValueOrDefault(project.PrimaryEditorId, "No Editor Assigned")
                    : "No Editor Assigned";
                project.SecondaryEditorName = project.SecondaryEditorId != null
                    ? userNames.GetValueOrDefault(project.SecondaryEditorId, "No Editor Assigned")
                    : "No Editor Assigned";

            }

            await context.SaveChangesAsync();
            return project;
        }
    }

    // fetching projects for users:
    public async Task<List<Project>> GetProjectsAsync()
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            return await context.Projects
                .Include(u => u.Client)
                .ToListAsync();
        }
    }

    public async Task<List<Project?>> GetProjectsForEditors(bool isArchived, string userId)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var projects = await context.Projects
                .Where(p => p.IsArchived == isArchived &&
                            ( p.PrimaryEditorId == userId || p.SecondaryEditorId == userId ))
                .Include(p => p.Archive)
                .Include(p => p.Client)
                .Include(p => p.PrimaryEditor)
                .Include(p => p.SecondaryEditor)
                .Include(p => p.Revisions)
                .OrderBy(p => p.InternalOrder)
                .ToListAsync();


            // Include PrimaryEditor only if the user is the primary editor
            foreach (var project in projects)
            {
                project.PrimaryEditorName = project.PrimaryEditorId != null
                                    ? project.PrimaryEditor!.UserName
                                    : "No Editor Assigned";
                project.SecondaryEditorName = project.SecondaryEditorId != null
                                    ? project.SecondaryEditor!.UserName
                                    : "No Editor Assigned";
                if (project.PrimaryEditorId != userId)
                {
                    project.PrimaryEditorDetails = null;
                }
                if (project.SecondaryEditorId != userId)
                {
                    project.SecondaryEditorDetails = null;
                }
            }
            return projects;
        }
    }

    public async Task<List<Project?>> GetProjectsForClients(bool isArchived, string UserId)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var projects = await context.Projects
            .Where(p => p.IsArchived == isArchived && p.ClientId == UserId)
            .Include(p => p.Archive)
            .Include(p => p.Revisions)
            .OrderBy(p => p.ExternalOrder)
            .ToListAsync();

            return projects;
        }
    }
    // For fetching chats
    public async Task<List<Project?>> GetProjectsForChat(bool isArchived, string userId,bool isAdminView, bool isEditorView, bool isClientView)
    {
        var stopwatch = Stopwatch.StartNew();

        await chatService.EnsureAdminChat(userId); // creates a 1 admin chat if it doesn't exist for each user that isn't admin.
        using (var context = _contextFactory.CreateDbContext())
        {
            // Query projects with pinned status for the user
            
            var projects = await context.Projects
                .Where(p => p.IsArchived == isArchived &&
                        ( isAdminView ? true :
                         isEditorView ? ( p.PrimaryEditorId == userId || p.SecondaryEditorId == userId ) :
                         isClientView ? p.ClientId == userId : false ))
                .Include(p => p.Client)
                .Include(p => p.PrimaryEditor)
                .Include(p => p.SecondaryEditor)
                .OrderBy(p => p.InternalOrder)
                .Include(p => p.Chat)
                .Select(p => new
                {
                    Project = p,
                    IsPinned = p.PinnedByUsers.Any(up => up.UserId == userId && up.IsPinned)
                })
                .ToListAsync();
            Console.WriteLine($"Fetched Projects Query - {stopwatch.ElapsedMilliseconds} ms");
            stopwatch.Restart();

            // Map the results to include the IsPinned property
            var result = projects.Select(x =>
            {
                x.Project.IsPinned = x.IsPinned; // Set the IsPinned property
                return x.Project;
            }).ToList();
            Console.WriteLine(value: $"Mapping done for pinned property - {stopwatch.ElapsedMilliseconds} ms");

            if (isAdminView)
            {
                Console.WriteLine($"Fetched Projects Query - {stopwatch.ElapsedMilliseconds} ms");
                stopwatch.Restart();

                // For admins, also get all user-admin chats as pseudo-projects
                List<Project>? adminChats = await context.Chats
                    .Where(c => c.IsAdminChat)
                    .Include(c => c.User)
                    .Select(c => new Project
                    {
                        ProjectId = -c.ChatId, // Using negative IDs to avoid conflicts
                        ProjectName = $"Management Chat",   
                        ClientId = c.UserId,
                        Client = c.User,
                    })
                    .ToListAsync();


                var finalList = result;
                Console.WriteLine($"Create the pseudo projects ## - {stopwatch.ElapsedMilliseconds} ms");
                stopwatch.Restart();

                finalList.InsertRange(0, adminChats);
                Console.WriteLine($"Inserting final list  - {stopwatch.ElapsedMilliseconds} ms");
                stopwatch.Stop();

                return finalList;
            }
            else
            {
                // For users to get their admin chat as pseudo-projects
                List<Project>? adminChat = await context.Chats
                    .Where(c => c.IsAdminChat && c.UserId == userId)
                    .Include(c => c.User)
                    .Select(c => new Project
                    {
                        ProjectId = -c.ChatId, // Using negative IDs to avoid conflicts
                        ProjectName = $"Management Chat",
                        ClientId = c.UserId,
                        Client = c.User,
                    })
                    .ToListAsync();


                var finalList = result;
                finalList.InsertRange(0, adminChat);
                stopwatch.Stop();
                return finalList;
            }
        }
    }
    // Lock to handle concurrency issue
    private static readonly object _orderLock = new object();
    public async Task AddProjectAsync(Project project)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == project.ClientId);

            if (user == null)
            {
                // Handle the case where the client does not exist.
                throw new Exception("Client not found.");
            }

            // If the project doesn't already exist
            var existingProject = await context.Projects.FirstOrDefaultAsync(p => p.ProjectId == project.ProjectId);

            if (existingProject == null)
            {
                project.ClientId = user.Id;  // Ensure ClientId is assigned to the project
                                             // Ensure that no two projects if created concurrently will have the same InternalOrder
                project.ProjectName = project.ProjectName.Trim();
                lock (_orderLock)
                {
                    _logger.LogInformation("Lock acquired for InternalOrder assignment.");
                    var highestOrder = context.Projects
                        .Where(p => !p.IsArchived)
                        .Max(p =>  p.InternalOrder) ?? 0;
                    project.InternalOrder = highestOrder + 1;

                    var highestOrderForClient = context.Projects
                        .Where(p => !p.IsArchived && project.ClientId == p.ClientId)
                        .Max(p =>  p.ExternalOrder) ?? 0;
                    project.ExternalOrder = highestOrderForClient + 1;

                    _logger.LogInformation("Lock released for InternalOrder assignment.");

                }
                // Add the project to the context and save changes
                context.Projects.Add(project);
                await context.SaveChangesAsync();

            }
            else
            {
                // Handle the case where the project already exists (optional)
                throw new Exception("Project already exists.");
            }
        }
    }

    public async Task AssignProjectToClientAsync(int projectId, string newUserId)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var _project = await context.Projects.AsTracking().FirstOrDefaultAsync(p => p.ProjectId == projectId);

            // Ensure the new client (user) exists
            var client = await context.Users.FirstOrDefaultAsync(u => u.Id == newUserId);

            if (client != null && _project != null)
            {

                // Update the ClientId property to the new client's ID
                _project.ClientId = client.Id;

                _project.DueDate = _project.ShootDate!.Value.AddDays((double) client.WeeksToDueDateDefault! * 7);

                var highestOrderForClient = context.Projects
                    .Where(p => !p.IsArchived && _project.ClientId == p.ClientId)
                    .Max(p =>  p.ExternalOrder) ?? 0;
                _project.ExternalOrder = highestOrderForClient + 1;

                await context.SaveChangesAsync();
            }
            else
            {
                throw new ArgumentException("Project or client not found.");
            }
        }
    }

    public async Task AssignProjectToPrimaryEditorAsync(int projectId, string userId)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var _project = await context.Projects.FindAsync(projectId);
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null && _project != null)
            {
                _project.PrimaryEditorId = userId;
                context.Projects.Update(_project);
                await context.SaveChangesAsync();
            }
            else
            {
                throw new ArgumentException("Project/User not found");
            }
        }
    }

    public async Task AssignProjectToSecondaryEditorAsync(string userId, string projectId)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var _project = await context.Projects.FindAsync(projectId);
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null && _project != null)
            {
                _project.SecondaryEditorId = userId;
                context.Projects.Update(_project);
                await context.SaveChangesAsync();
            }
            else
            {
                throw new ArgumentException("Project/User not found");
            }
        }
    }
    public async Task UpdateProjectAsync(ExtendedCalendarItem calendarItem, string updatedByUserId, bool isAdmin)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var _project = await context.Projects
                .AsTracking()
                .Include(p => p.PrimaryEditorDetails)
                .Include(p => p.SecondaryEditorDetails)
                .Include(p => p.Client)
                .FirstOrDefaultAsync(p => p.ProjectId == calendarItem.ProjectId);
            if(_project == null)
            {
                return;
            }
            _project.StartDate = calendarItem.Start;
            _project.EndDate = calendarItem.End;

            await context.SaveChangesAsync();

            if (isAdmin == false)
                _ = Task.Run(() => _notificationService.QueueProjectScheduleUpdated(_project, updatedByUserId));


        }

    }
    public async Task UpdateProjectAsync(Project project, string updatedByUserId)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var isExternalOrderChanged = false;
            var _project = await context.Projects
                .AsTracking()
                .Include(p => p.Revisions)
                .Include(p => p.PrimaryEditorDetails)
                .Include(p => p.SecondaryEditorDetails)
                .Include(p => p.Client)
                .FirstOrDefaultAsync(p => p.ProjectId == project.ProjectId);
            if (_project != null)
            {
                var oldStatus = _project.Status;

                if (_project.ClientId != project.ClientId)
                {
                    if (project.Client.WeeksToDueDateDefault == null)
                    {
                        project.Client.WeeksToDueDateDefault = 8;
                    }
                    _project.ClientId = project.ClientId;
                    var highestOrderForClient = context.Projects
                        .Where(p => !p.IsArchived && _project.ClientId == p.ClientId)
                        .Max(p =>  p.ExternalOrder) ?? 0;
                    _project.ExternalOrder = highestOrderForClient + 1;
                    isExternalOrderChanged = true;
                }
                if (_project.InternalOrder != project.InternalOrder && _project.InternalOrder != null)
                {
                    await ReorderProjectAsync(project.ProjectId, project.InternalOrder!.Value, false);
                }
                if (_project.ExternalOrder != project.ExternalOrder && _project.ExternalOrder != null)
                {
                    await ReorderProjectAsync(project.ProjectId, project.ExternalOrder!.Value, true);
                }
                if (_project.PrimaryEditorId != project.PrimaryEditorId && _project.PrimaryEditorId == null)
                {
                    _project.PrimaryEditorId = project.PrimaryEditorId;
                }
                bool hasPrimaryEditorChanged = _project.PrimaryEditorId != null && _project.PrimaryEditorId != project.PrimaryEditorId;
                bool isNewPrimaryEditorNull = project.PrimaryEditorId == null;

                if (hasPrimaryEditorChanged && isNewPrimaryEditorNull)
                {
                    await loggingHours.DeleteLoggedHoursAsync(_project.ProjectId, _project.PrimaryEditorId!);
                    _project.PrimaryEditorId = project.PrimaryEditorId;
                }
                bool hasSecondaryEditorChanged = _project.SecondaryEditorId != null && _project.SecondaryEditorId != project.SecondaryEditorId;
                bool isNewSecondaryEditorNull = project.SecondaryEditorId == null;
                bool recalculatePayment = false;
                if (hasSecondaryEditorChanged && isNewSecondaryEditorNull)
                {
                    // this deletes logged house for promoted user from secondary to primary,
                    // as a fix for this, we check if the user is promoted from secondary to primary
                    // if so, we don't delete the logged hours for the user
                    if (_project.SecondaryEditorId == project.PrimaryEditorId && project.SecondaryEditorId == null)
                    {
                        if (_project.SecondaryEditorId != _project.PrimaryEditorId)
                        {
                            await loggingHours.DeleteLoggedHoursAsync(_project.ProjectId, _project.PrimaryEditorId!); // delete logged hours for the primary editor before promoting the secondary editor
                        }
                        _project.PrimaryEditorId = project.PrimaryEditorId;
                        _project.SecondaryEditorId = project.SecondaryEditorId;
                        recalculatePayment = true;

                    }
                    else
                    {
                        await loggingHours.DeleteLoggedHoursAsync(_project.ProjectId, _project.SecondaryEditorId!);
                        _project.SecondaryEditorId = project.SecondaryEditorId;
                    }
                }
                context.Entry(_project).CurrentValues.SetValues(project);
                if (project.PrimaryEditorDetails != null)
                {
                    context.Entry(_project.PrimaryEditorDetails).CurrentValues.SetValues(project.PrimaryEditorDetails);
                }

                if (project.SecondaryEditorDetails != null)
                {
                    context.Entry(_project.SecondaryEditorDetails).CurrentValues.SetValues(project.SecondaryEditorDetails);
                }

                if (project.ProjectSpecifications != null)
                {
                    context.Entry(_project.ProjectSpecifications).CurrentValues.SetValues(project.ProjectSpecifications);
                }

                if (project.Revisions != null)
                {
                    foreach (var revision in project.Revisions)
                    {
                        // Ensure the revision is not null and sort it by RevisionId and RevisionDate
                        var existingRevision = _project.Revisions
                            .FirstOrDefault(r => r.RevisionId == revision!.RevisionId && r.RevisionDate == revision.RevisionDate);

                        // Define a method to check if content is effectively empty (only contains <p><br></p> or similar empty content)
                        bool IsContentEmpty(string content)
                        {
                            // Trim to remove any leading/trailing spaces and check if the content is just the empty paragraph or contains only whitespace
                            return string.IsNullOrWhiteSpace(content) || content.Trim() == "<p><br></p>";
                        }

                        if (existingRevision != null)
                        {
                            // If the revision already exists, check if content is empty and remove the revision if necessary
                            if (IsContentEmpty(revision.Content))
                            {
                                // Remove the entire revision if content is empty or only <p><br></p>
                                _project.Revisions.Remove(existingRevision);
                                
                            }
                            else
                            {
                                // If the revision content is not empty, update the revision details
                                existingRevision.Content = revision.Content;
                                existingRevision.RevisionDate = revision.RevisionDate;
                                existingRevision.isCompleted = revision.isCompleted;
                            }
                        }
                        else
                        {
                            // If the revision is new and the content is not empty, add it to the project
                            if (!IsContentEmpty(revision.Content))
                            {
                                _project.Revisions.Add(revision);
                                _project.Status = ProjectStatus.Revision;
                                _project.ProgressBar = 0;
                            }
                        }
                    }

                }


                await context.SaveChangesAsync();

                if(recalculatePayment)
                {
                    await CalculateProjectFinalPrice(_project);

                }
                // For reordering the projects if the external order has changed and is not valid
                if (isExternalOrderChanged)
                {
                    var externalProjectsToReorder = await context.Projects
                        .AsTracking()
                        .Where(p => !p.IsArchived && p.ClientId == project.ClientId)
                        .OrderBy(p => p.ExternalOrder)
                        .ToListAsync();
                    var isExternalOrderValid = IsProjectOrderValid(externalProjectsToReorder, true);
                    if (!isExternalOrderValid)
                        NormalizeProjectOrder(externalProjectsToReorder, isExternalOrder: true);
                    await context.SaveChangesAsync();

                }
                // Send notification to user if the project status has changed
                if (oldStatus != _project.Status)
                {
                    var userIds = new List<string?> { _project.PrimaryEditorId, _project.SecondaryEditorId }
                        .Where(id => id != null)
                        .Distinct()
                        .ToList();

                    var userNames = await context.Users
                        .Where(u => userIds.Contains(u.Id))
                        .Select(u => new { u.Id, u.UserName, u.HourlyRate })
                        .ToDictionaryAsync(u => u.Id, u => u.UserName);

                    _project.PrimaryEditorName = project.PrimaryEditorId != null && userNames.ContainsKey(project.PrimaryEditorId)
                        ? userNames[project.PrimaryEditorId]!
                        : null;
                    _project.SecondaryEditorName = project.SecondaryEditorId != null && userNames.ContainsKey(project.SecondaryEditorId)
                        ? userNames[project.SecondaryEditorId]!
                        : null;

                    string messageToSend = $"Project {_project.ProjectName} status has been changed from {oldStatus.ToString().Replace('_', ' ')} to {_project.Status.ToString().Replace('_', ' ')}.";
                    if (_project.Status != ProjectStatus.Ready_To_Review && _project.Status != ProjectStatus.Ready_To_Edit && oldStatus != ProjectStatus.Ready_To_Edit && oldStatus != ProjectStatus.Ready_To_Review)
                    {
                        _ = Task.Run(() => chatService.AddMessageAsync(_project.Client.Id, updatedByUserId, messageToSend));
                    }
                    else if (
                        (  _project.Status == ProjectStatus.Ready_To_Review || _project.Status == ProjectStatus.Working )
                        && (oldStatus == ProjectStatus.Ready_To_Review || oldStatus == ProjectStatus.Working ))
                    {
                        // Do nothing since we don't want to send info the user at all (intentionally left blank)
                    }
                    else
                    {
                        messageToSend = messageToSend.Replace("Ready To Review", "Working");
                        _ = Task.Run(() => chatService.AddMessageAsync(_project.Client.Id, updatedByUserId, messageToSend));

                    }
                    _ = Task.Run(() => _notificationService.QueueStatusChangeNotification(_project, oldStatus, _project.Status, updatedByUserId));
                }
                if(hasPrimaryEditorChanged || hasSecondaryEditorChanged)
                {
                    _ = Task.Run(() => loggingHours.CalculateAndSaveBillableHoursForProjectAsync(_project.ProjectId));
                }
                _ = Task.Run(() =>  _broadcaster.NotifyAllAsync());
            }
        }
    }
    public async Task UpdateProjectsInBatchAsync(IEnumerable<Project> projects, string updatedByUserId)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var projectIds = projects.Select(p => p.ProjectId).ToList();

            // Load all affected projects with their relations
            var dbProjects = await context.Projects
                .AsTracking()
                .Include(p => p.Revisions)
                .Include(p => p.PrimaryEditorDetails)
                .Include(p => p.SecondaryEditorDetails)
                .Include(p => p.Client)
                .Where(p => projectIds.Contains(p.ProjectId))
                .ToDictionaryAsync(p => p.ProjectId);

            var statusChanges = new List<(Project Project, ProjectStatus OldStatus, ProjectStatus NewStatus)>();

            foreach (var project in projects)
            {
                if (!dbProjects.TryGetValue(project.ProjectId, out var dbProject))
                    continue;

                var oldStatus = dbProject.Status;

                // Update project values
                context.Entry(dbProject).CurrentValues.SetValues(project);

                // Handle status changes
                if (oldStatus != project.Status)
                {
                    if (project.Status == ProjectStatus.Delivered)
                    {
                        dbProject.AdminStatus = AdminProjectStatus.Delivered_Not_Paid;
                    }
                    statusChanges.Add((dbProject, oldStatus, project.Status));
                }

                // Update related entities
                if (project.PrimaryEditorDetails != null)
                {
                    dbProject.PrimaryEditorDetails.DatePaidEditor = project.PrimaryEditorDetails.DatePaidEditor!;
                }

                if (project.SecondaryEditorDetails != null)
                {
                    dbProject.SecondaryEditorDetails.DatePaidEditor = project.SecondaryEditorDetails.DatePaidEditor!;
                }

                if (project.ProjectSpecifications != null)
                {
                    context.Entry(dbProject.ProjectSpecifications).CurrentValues.SetValues(project.ProjectSpecifications);
                }
            }

            // Save all changes
            await context.SaveChangesAsync();

            // Send notifications for status changes
            foreach (var (project, oldStatus, newStatus) in statusChanges)
            {
                var allUserIds = projects
                    .SelectMany(p => new[] { p.PrimaryEditorId, p.SecondaryEditorId })
                    .Where(id => id != null)
                    .Distinct()
                    .ToList();

                var userNames = await context.Users
                    .Where(u => allUserIds.Contains(u.Id))
                    .Select(u => new { u.Id, u.UserName })
                    .ToDictionaryAsync(u => u.Id, u => u.UserName);

                foreach(Project _project in projects)
                {
                    _project.PrimaryEditorName = project.PrimaryEditorId != null && userNames.ContainsKey(project.PrimaryEditorId)
                            ? userNames[project.PrimaryEditorId]!
                            : null;
                    _project.SecondaryEditorName = project.SecondaryEditorId != null && userNames.ContainsKey(project.SecondaryEditorId)
                        ? userNames[project.SecondaryEditorId]!
                        : null;
                }


                string messageToSend = $"Project {project.ProjectName} status has been changed from {oldStatus.ToString().Replace('_', ' ')} to {project.Status.ToString().Replace('_', ' ')}.";
                if (project.Status != ProjectStatus.Ready_To_Review && project.Status != ProjectStatus.Ready_To_Edit && oldStatus != ProjectStatus.Ready_To_Edit && oldStatus != ProjectStatus.Ready_To_Review)
                {
                    _ = Task.Run(() => chatService.AddMessageAsync(project.Client.Id, updatedByUserId, messageToSend));
                }
                else if (
                    ( project.Status == ProjectStatus.Ready_To_Review || project.Status == ProjectStatus.Working )
                    && ( oldStatus == ProjectStatus.Ready_To_Review || oldStatus == ProjectStatus.Working ))
                {
                    // Do nothing since we don't want to send info the user at all (intentionally left blank)
                }
                else
                {
                    messageToSend = messageToSend.Replace("Ready To Review", "Working");
                    _ = Task.Run(() => chatService.AddMessageAsync(project.Client.Id, updatedByUserId, messageToSend));

                }
                _ = Task.Run(() => _notificationService.QueueStatusChangeNotification(project, oldStatus, newStatus, updatedByUserId));
            }

            // Broadcast changes
            await _broadcaster.NotifyAllAsync();
        }
    }
    public async Task TogglePinAsync(string userId, int projectId, bool isPinned)
    {
        using var context = _contextFactory.CreateDbContext();

        var retryCount = 3;
        while (retryCount > 0)
        {
            try
            {
                // Begin a transaction with Serializable isolation level
                using var transaction = await context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

                // Check for existing pin
                var existingPin = await context.UserProjectPins
                    .AsTracking()
                    .FirstOrDefaultAsync(up => up.UserId == userId && up.ProjectId == projectId);

                if (existingPin != null)
                {
                    existingPin.IsPinned = isPinned; // Update existing entry
                }
                else
                {
                    context.UserProjectPins.Add(new UserProjectPin
                    {
                        UserId = userId,
                        ProjectId = projectId,
                        IsPinned = isPinned
                    });
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync(); // Commit the transaction
                break; // Exit the retry loop if successful
            }
            catch (DbUpdateException ex) when (IsConcurrencyConflict(ex))
            {
                // Handle concurrency conflict
                retryCount--;
                if (retryCount == 0)
                {
                    throw; // Re-throw the exception if retries are exhausted
                }
            }
        }
    }



    public async Task UpdateProjectBillableHoursAsync(Project project)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var _project = await context.Projects
                .AsTracking()
                .Include(p => p.Client)
                .Include(p => p.CalculationDetails)
                .FirstAsync(p => p.ProjectId == project.ProjectId);
            if (_project != null)
            {
                _project.ClientBillableHours = project.ClientBillableHours.HasValue
                    ? Math.Round(project.ClientBillableHours.Value, 2)
                    :  null;

                _project.ClientBillableAmount = Math.Round(( project.Client.HourlyRate ?? 0 ) * ( project.ClientBillableHours ?? 0 ), 2);

                _project.PrimaryEditorDetails.FinalBillableHours = Math.Round(project.PrimaryEditorDetails.FinalBillableHours, 2);

                _project.PrimaryEditorDetails.AdjustmentHours = Math.Round(project.PrimaryEditorDetails.AdjustmentHours, 2);

                _project.SecondaryEditorDetails.FinalBillableHours = Math.Round(project.SecondaryEditorDetails.FinalBillableHours, 2);

                _project.SecondaryEditorDetails.AdjustmentHours = Math.Round(project.SecondaryEditorDetails.AdjustmentHours, 2);

                foreach (var property in typeof(ProjectCalculationDetails).GetProperties())
                {
                    var newValue = property.GetValue(project.CalculationDetails);
                    property.SetValue(_project.CalculationDetails, newValue);
                }
                _project.SubmissionStatus = project.SubmissionStatus;
                context.Projects.Update(_project);
                await context.SaveChangesAsync();
            }
            else
            {
                throw new ArgumentException("Project not found");
            }
        }
    }

    public async Task DeleteProjectAsync(int projectId)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var project = await context.Projects.FindAsync(projectId);
            if (project.ProjectId == projectId)
            {
                context.Projects.Remove(project);
                await context.SaveChangesAsync();
                await _broadcaster.NotifyAllAsync();
            }
        }
    }

    public async Task DeleteProjectsAsync(List<int> projectId)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var projects = await context.Projects
                .Where(p => projectId.Contains(p.ProjectId))
                .ExecuteDeleteAsync();

                await _broadcaster.NotifyAllAsync();
        }
    }
    public async Task ArchiveProjectAsync(int projectId, string reason)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var project = await context.Projects
                .AsTracking()
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);

            if (project == null || project.IsArchived)
            {
                return;
            }

            var clientId = project.ClientId;

            // Archive the project
            project.IsArchived = true;
            project.InternalOrder = null;
            project.ExternalOrder = null;
            project.Archive = new Archive
            {
                ProjectId = projectId,
                Reason = reason ?? string.Empty,
                ArchiveDate = DateTime.UtcNow
            };
            await context.SaveChangesAsync();
            // Reorder external (client-specific) projects
            var externalProjectsToReorder = await context.Projects
                .AsTracking()
                .Where(p => !p.IsArchived && p.ClientId == clientId)
                .OrderBy(p => p.ExternalOrder)
                .ToListAsync();

            var isExternalOrderValid = IsProjectOrderValid(externalProjectsToReorder, true);
            if (!isExternalOrderValid)
                NormalizeProjectOrder(externalProjectsToReorder, isExternalOrder: true);

            // Reorder internal projects
            var internalProjectsToReorder = await context.Projects
                .AsTracking()
                .Where(p => !p.IsArchived)
                .OrderBy(p => p.InternalOrder)
                .ToListAsync();
            var isInternalOrderValid = IsProjectOrderValid(externalProjectsToReorder, false);
            if (!isInternalOrderValid)
                NormalizeProjectOrder(internalProjectsToReorder, isExternalOrder: false);
            try
            {
                await context.SaveChangesAsync();
                await _broadcaster.NotifyAllAsync();
            }
            catch (Exception ex)
            {
                // Log the error or handle it according to your error handling strategy
                throw new InvalidOperationException("Failed to archive project", ex);
            }
        }
    }

    public async Task ArchiveProjectsAsync(List<int> projectIds, ArchiveReason? reason)
    {
        using var context = _contextFactory.CreateDbContext();

        if (projectIds == null || projectIds.Count == 0)
            return;

        // Get distinct client IDs without pulling full entities
        var clientIds = await context.Projects
            .Where(p => projectIds.Contains(p.ProjectId) && !p.IsArchived)
            .Select(p => p.ClientId)
            .Distinct()
            .ToListAsync();

        if (clientIds.Count == 0)
            return;

        // Archive projects in batch (efficient update)
        await context.Projects
            .Where(p => projectIds.Contains(p.ProjectId) && !p.IsArchived)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.IsArchived, true)
                .SetProperty(p => p.InternalOrder, (int?) null)
                .SetProperty(p => p.ExternalOrder, (int?) null)
            );

        // Insert archive records in batch without loading projects
        var archiveRecords = projectIds.Select(id => new Archive
        {
            ProjectId = id,
            Reason = reason.ToString() ?? string.Empty,
            ArchiveDate = DateTime.UtcNow
        }).ToList();

        await context.Archives.AddRangeAsync(archiveRecords);
        await context.SaveChangesAsync();

        // Reorder external projects (grouped by ClientId)
        var externalProjectsGroupedByClient = await context.Projects
            .Where(p => !p.IsArchived && clientIds.Contains(p.ClientId))
            .OrderBy(p => p.ExternalOrder)
            .GroupBy(p => p.ClientId)
            .ToListAsync();

        foreach (var clientProjects in externalProjectsGroupedByClient)
        {
            var projectList = clientProjects.ToList();
            if (!IsProjectOrderValid(projectList, true))
                NormalizeProjectOrder(projectList, isExternalOrder: true);
        }

        // Reorder internal projects (all together)
        var internalProjectsToReorder = await context.Projects
            .Where(p => !p.IsArchived)
            .OrderBy(p => p.InternalOrder)
            .ToListAsync();

        if (!IsProjectOrderValid(internalProjectsToReorder, false))
            NormalizeProjectOrder(internalProjectsToReorder, isExternalOrder: false);

        await context.SaveChangesAsync();
        await _broadcaster.NotifyAllAsync();
    }

    public async Task UnarchiveProjectAsync(int projectId)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var project = await context.Projects.AsTracking().FirstOrDefaultAsync(p => p.ProjectId == projectId);
            if (project != null && project.IsArchived)
            {
                await DeleteArchiveAsync(projectId);
                lock (_orderLock)
                {
                    _logger.LogInformation("Lock acquired for InternalOrder assignment.");
                    var highestOrder = context.Projects
                        .Where(p => !p.IsArchived)
                        .Max(p =>  p.InternalOrder) ?? 0;

                    project.InternalOrder = highestOrder + 1;

                    var highestOrderClient = context.Projects
                        .Where(p => !p.IsArchived && p.ClientId == project.ClientId)
                        .Max(p =>  p.ExternalOrder) ?? 0;

                    project.ExternalOrder = highestOrderClient + 1;
                    _logger.LogInformation("Lock released for InternalOrder assignment.");

                }
                project.IsArchived = false;
                await context.SaveChangesAsync();
                await _broadcaster.NotifyAllAsync();
            }
            else
            {
                throw new ArgumentException("Project or client not found.");
            }
        }
    }

    public async Task DeleteArchiveAsync(int projectId)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var archiveRecord = await context.Archives
            .FirstOrDefaultAsync(a => a.ProjectId == projectId);

            if (archiveRecord != null)
            {
                context.Archives.Remove(archiveRecord);
                await context.SaveChangesAsync();
                await _broadcaster.NotifyAllAsync();
            }
        }
    }
    public async Task ReorderProjectAsync(int projectId, int? newOrder, bool isExternalOrder)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                List<Project> projectsToReorder = new();
                // Fetch all non-archived projects
                if (isExternalOrder)
                {
                    var project = await context.Projects.AsTracking().FirstOrDefaultAsync(p => p.ProjectId == projectId);
                    var clientId = project.ClientId;

                    projectsToReorder = await context.Projects
                        .AsTracking()
                        .Where(p => p.IsArchived == false && p.ClientId == clientId)
                        .OrderBy(p => isExternalOrder ? p.ExternalOrder : p.InternalOrder)
                        .ToListAsync();
                }
                else
                {
                    projectsToReorder = await context.Projects
                        .AsTracking()
                        .Where(p => p.IsArchived == false)
                        .OrderBy(p => isExternalOrder ? p.ExternalOrder : p.InternalOrder)
                        .ToListAsync();
                }
                // Validate input order
                ValidateNewOrder(projectsToReorder, newOrder);

                // If the list is empty or has unexpected order, normalize it
                if (!projectsToReorder.Any() || !IsProjectOrderValid(projectsToReorder, isExternalOrder))
                {
                    NormalizeProjectOrder(projectsToReorder, isExternalOrder);
                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return;
                }

                // Find the project being moved
                var projectToMove = projectsToReorder.FirstOrDefault(p => p.ProjectId == projectId);
                if (projectToMove == null)
                    return;

                // If newOrder is null, perform a full reordering
                if (newOrder == null)
                {
                    NormalizeProjectOrder(projectsToReorder, isExternalOrder);
                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return;
                }

                // Perform the reordering
                PerformProjectReordering(projectsToReorder, projectToMove, newOrder.Value, isExternalOrder);

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                // Notify all clients of the change in project order
                await _broadcaster.NotifyAllAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
    private bool IsProjectOrderValid(List<Project> projects, bool isExternalOrder)
    {
        // Validate if projects are in a continuous sequence based on ExternalOrder or InternalOrder
        return projects
            .Select(p => isExternalOrder ? p.ExternalOrder ?? 0 : p.InternalOrder ?? 0)
            .OrderBy(o => o)
            .Select((order, index) => order == index + 1)
            .All(isValid => isValid);
    }

    private void NormalizeProjectOrder(List<Project> projects, bool isExternalOrder)
    {
        // Sort projects by their current order (ExternalOrder or InternalOrder), then assign new sequential order
        var orderedProjects = projects
            .OrderBy(p => isExternalOrder ? p.ExternalOrder ?? int.MaxValue : p.InternalOrder ?? int.MaxValue)
            .ToList();

        for (int i = 0; i < orderedProjects.Count; i++)
        {
            if (isExternalOrder)
                orderedProjects[i].ExternalOrder = i + 1;
            else
                orderedProjects[i].InternalOrder = i + 1;
        }
    }
    private void PerformProjectReordering(List<Project> projects, Project projectToMove, int newOrder, bool isExternalOrder)
    {
        // Remove the project from its current position
        projects.Remove(projectToMove);

        // Insert the project at the new position
        projects.Insert(newOrder - 1, projectToMove);

        // Reassign orders based on ExternalOrder or InternalOrder
        for (int i = 0; i < projects.Count; i++)
        {
            if (isExternalOrder)
                projects[i].ExternalOrder = i + 1;
            else
                projects[i].InternalOrder = i + 1;
        }
    }
    private void ValidateNewOrder(List<Project> projects, int? newOrder)
    {
        if (newOrder == null)
            return;

        // Check if new order is within valid range
        if (newOrder < 1 || newOrder > projects.Count + 1)
            throw new InvalidProjectOrderException($"Invalid order. Must be between 1 and {projects.Count + 1}.");
    }
    // Calculate the final price of the project based on the billable hours of the client and editors assigned to the project 
    // Where Project.ClientBillableHours is the total billable hours for the client 
    // and Project.ClientBillableAmount is the total amount to be paid by the client
    // Project.PrimaryEditorDetails.BillableHours is the total billable hours for the primary editor
    // Project.SecondaryEditorDetails.BillableHours is the total billable hours for the secondary editor
    // Project.PrimaryEditorDetails.Overtime is the difference between the primary editor's billable hours and the client's billable hours
    // Project.SecondaryEditorDetails.Overtime is the difference between the secondary editor's billable hours and the client's billable hours
    // Project.PrimaryEditorDetails.FinalBillableHours is the total billable hours for the primary editor after adjusting for overtime and adjustment hours
    // Project.SecondaryEditorDetails.FinalBillableHours is the total billable hours for the secondary editor after adjusting for overtime and adjustment hours
    // Project.PrimaryEditorDetails.PaymentAmount is the total amount to be paid to the primary editor
    // Project.SecondaryEditorDetails.PaymentAmount is the total amount to be paid to the secondary editor

    public async Task CalculateProjectFinalPrice(Project _project)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                await loggingHours.CalculateAndSaveBillableHoursForProjectAsync(_project.ProjectId);
                var project = await context.Projects
                    .AsTracking()
                    .Include(p => p.Client)
                    .Include(p => p.PrimaryEditor)
                    .Include(p => p.SecondaryEditor)
                    .Include(p => p.PrimaryEditorDetails)
                    .Include(p => p.SecondaryEditorDetails)
                    .FirstOrDefaultAsync(p => p.ProjectId == _project.ProjectId);
                if(project == null)
                    throw new ArgumentException("Project not found");
                // Calculate client billable amount
                if (project.ClientBillableHours != null && project.Client.HourlyRate != null)
                {
                    project.ClientBillableAmount =  project.Client.HourlyRate.HasValue && project.ClientBillableHours.HasValue 
                        ? Math.Round(project.Client.HourlyRate.Value * project.ClientBillableHours.Value, 0, MidpointRounding.AwayFromZero)
                        :  null;
                }

                // Calculate primary editor details
                if (project.PrimaryEditor != null)
                {
                    CalculateEditorPayment(project.PrimaryEditorDetails, project.PrimaryEditor, project.ClientBillableHours);
                    context.Entry(project.PrimaryEditorDetails).State = EntityState.Modified;
                }
                else if(project.PrimaryEditor == null)
                {
                    ResetEditorDetails(project.PrimaryEditorDetails);
                }


                // Calculate secondary editor details
                if (project.SecondaryEditor != null && project.SecondaryEditorDetails.BillableHours != null)
                {
                    CalculateEditorPayment(project.SecondaryEditorDetails, project.SecondaryEditor, project.ClientBillableHours);
                    context.Entry(project.SecondaryEditorDetails).State = EntityState.Modified;
                }
                else if(project.SecondaryEditor == null)
                {
                    ResetEditorDetails(project.SecondaryEditorDetails);
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                _ = Task.Run(() => _broadcaster.NotifyAllAsync());

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Bug in calculating total project price in {nameof(CalculateProjectFinalPrice)}: \n\n {ex} ");
            }
        }
    }

    private static void CalculateEditorPayment(EditorDetails editorDetails, ApplicationUser editor, decimal? clientBillableHours)
    {
        editorDetails.Overtime = Math.Round(( editorDetails.BillableHours ) - ( editorDetails.FinalBillableHours ), 2);

        editorDetails.FinalBillableHours = Math.Round(
            ( editorDetails.BillableHours) -
            ( editorDetails.Overtime ) +
            ( editorDetails.AdjustmentHours ),
            2
        );

        editorDetails.PaymentAmount = Math.Round(
            ( editor.HourlyRate ?? 0 ) * ( editorDetails.FinalBillableHours),
            0, MidpointRounding.AwayFromZero);

    }
    private static void ResetEditorDetails(EditorDetails editorDetails)
    {
        editorDetails.BillableHours = 0;
        editorDetails.FinalBillableHours = 0;
        editorDetails.Overtime = 0;
        editorDetails.PaymentAmount = null;
        editorDetails.AdjustmentHours = 0;
    }

}