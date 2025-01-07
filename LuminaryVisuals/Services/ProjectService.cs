namespace LuminaryVisuals.Services;
using LuminaryVisuals.Data;
using LuminaryVisuals.Data.Entities;
using LuminaryVisuals.Models;
using LuminaryVisuals.Services.Events;
using LuminaryVisuals.Services.Mail;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ProjectService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ILogger<ProjectService> _logger;
    private readonly CircuitUpdateBroadcaster _broadcaster;
    private readonly INotificationService _notificationService;
    public ProjectService(IDbContextFactory<ApplicationDbContext> context, ILogger<ProjectService> logger,
        CircuitUpdateBroadcaster projectUpdateService, INotificationService notificationService)
    {
        _contextFactory = context;
        _logger = logger;
        _broadcaster = projectUpdateService;
        _notificationService = notificationService;
    }

    public async Task<List<Project>> GetProjectsAsync(bool isArchived, int itemsPerPage)
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

            var totalCount = projects.Count;
            var reorderedProjects = new List<Project>();

            // Get the chunks in reverse order but maintain original order within each chunk
            for (int startIndex = totalCount; startIndex > 0; startIndex -= itemsPerPage)
            {
                int skip = Math.Max(0, startIndex - itemsPerPage);
                int take = Math.Min(itemsPerPage, startIndex - skip);

                var chunk = projects.GetRange(skip, take);
                reorderedProjects.AddRange(chunk);
            }

            var userIds = reorderedProjects
                .SelectMany(p => new[] { p.ClientId, p.PrimaryEditorId, p.SecondaryEditorId })
                .Where(id => id != null)
                .Distinct()
                .ToList();

            var userNames = await context.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName, u.HourlyRate })
                .ToDictionaryAsync(u => u.Id, u => u.UserName);

            foreach (var project in reorderedProjects)
            {
                project.ClientName = project.ClientId != null && userNames.ContainsKey(project.ClientId)
                    ? userNames[project.ClientId]!
                    : "No Client Assigned ??";
                project.PrimaryEditorName = project.PrimaryEditorId != null && userNames.ContainsKey(project.PrimaryEditorId)
                    ? userNames[project.PrimaryEditorId]!
                    : "No Editor Assigned";
                project.SecondaryEditorName = project.SecondaryEditorId != null && userNames.ContainsKey(project.SecondaryEditorId)
                    ? userNames[project.SecondaryEditorId]!
                    : "No Editor Assigned";
            }

            await context.SaveChangesAsync();
            return reorderedProjects;
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

    // Used for dragging orders to update projects when dragged
    public async Task<List<Project>> GetProjectsAsync()
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            return await context.Projects
            .ToListAsync();
        }
    }
    public async Task<List<Project?>> GetProjectsForEditors(bool isArchived, string userId, int itemsPerPage)
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
            if (itemsPerPage > 0)
            {
                var totalCount = projects.Count;
                var reorderedProjects = new List<Project>();

                // Get the chunks in reverse order but maintain original order within each chunk
                for (int startIndex = totalCount; startIndex > 0; startIndex -= itemsPerPage)
                {
                    int skip = Math.Max(0, startIndex - itemsPerPage);
                    int take = Math.Min(itemsPerPage, startIndex - skip);

                    var chunk = projects.GetRange(skip, take);
                    reorderedProjects.AddRange(chunk);
                }
                return reorderedProjects;

            }
            return projects;
        }
    }

    public async Task<List<Project?>> GetProjectsForClients(bool isArchived, string UserId, int itemsPerPage)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var projects = await context.Projects
            .Where(p => p.IsArchived == isArchived && p.ClientId == UserId)
            .Include(p => p.Archive)
            .Include(p => p.Revisions)
            .OrderBy(p => p.ExternalOrder)
            .ToListAsync();
            if (itemsPerPage > 0)
            {
                var totalCount = projects.Count;
                var reorderedProjects = new List<Project>();

                // Get the chunks in reverse order but maintain original order within each chunk
                for (int startIndex = totalCount; startIndex > 0; startIndex -= itemsPerPage)
                {
                    int skip = Math.Max(0, startIndex - itemsPerPage);
                    int take = Math.Min(itemsPerPage, startIndex - skip);

                    var chunk = projects.GetRange(skip, take);
                    reorderedProjects.AddRange(chunk);
                }
                if (reorderedProjects == null)
                    return null;
                return reorderedProjects;
            }
            return projects;
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
                lock (_orderLock)
                {
                    _logger.LogInformation("Lock acquired for InternalOrder assignment.");
                    var highestOrder = context.Projects
                        .Where(p => !p.IsArchived)
                        .Max(p => (int?) p.InternalOrder) ?? 0;
                    project.InternalOrder = highestOrder + 1;

                    var highestOrderForClient = context.Projects
                        .Where(p => !p.IsArchived && project.ClientId == p.ClientId)
                        .Max(p => (int?) p.ExternalOrder) ?? 0;
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
                    .Max(p => (int?) p.ExternalOrder) ?? 0;
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

    public async Task UpdateProjectAsync(Project project, string updatedByUserId)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var _project = await context.Projects
                .AsTracking()
                .Include(p => p.Revisions)
                .Include(p => p.PrimaryEditorDetails)
                .Include(p => p.SecondaryEditorDetails)
                .FirstOrDefaultAsync(p => p.ProjectId ==  project.ProjectId);
            if (_project != null)
            {
                var isChanged = false;
                var oldStatus = _project.Status;
                if (_project.ClientId != project.ClientId)
                {
                    if (project.Client.WeeksToDueDateDefault == null)
                    {
                        project.Client.WeeksToDueDateDefault = 4;
                    }
                    _project.ClientId = project.ClientId;
                    var highestOrderForClient = context.Projects
                        .Where(p => !p.IsArchived && _project.ClientId == p.ClientId)
                        .Max(p => (int?) p.ExternalOrder) ?? 0;
                    _project.ExternalOrder = highestOrderForClient + 1;
                }
                if (_project.InternalOrder != project.InternalOrder && _project.InternalOrder != null)
                {
                    await ReorderProjectAsync(project.ProjectId, project.InternalOrder!.Value,false);
                }
                if (_project.ExternalOrder != project.ExternalOrder && _project.ExternalOrder != null)
                {
                    await ReorderProjectAsync(project.ProjectId, project.ExternalOrder!.Value,true);
                }
                if(_project.PrimaryEditorId != project.PrimaryEditorId && _project.PrimaryEditorId == null)
                {
                    _project.PrimaryEditorId = project.PrimaryEditorId;
                    isChanged = true;
                }
                context.Entry(_project).CurrentValues.SetValues(project);
                if(isChanged)
                {
                    _project.Status = ProjectStatus.Scheduled;
                }
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
                    var existingRevision = _project.Revisions
                        .FirstOrDefault(r => r.RevisionId == revision.RevisionId);

                    if (existingRevision != null)
                    {
                        // If the revision already exists, update its content
                        existingRevision.Content = revision.Content;
                        existingRevision.RevisionDate = revision.RevisionDate;
                    }
                    else
                    {
                        // If the revision is new, add it to the project
                        _project.Revisions.Add(revision);
                    }
                }
                }
                // Send notification to user if the project status has changed
                if (oldStatus != _project.Status)
                {
                    await _notificationService.QueueStatusChangeNotification(project, oldStatus, _project.Status,updatedByUserId);
                }

                await context.SaveChangesAsync();
                await _broadcaster.NotifyAllAsync();
                
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
                _project.ClientBillableHours = project.ClientBillableHours;
                _project.ClientBillableAmount = (project.Client.HourlyRate ?? 0 ) * project.ClientBillableHours;
                _project.PrimaryEditorDetails.FinalBillableHours = project.PrimaryEditorDetails.FinalBillableHours;
                _project.PrimaryEditorDetails.AdjustmentHours = project.PrimaryEditorDetails.AdjustmentHours;
                _project.SecondaryEditorDetails.FinalBillableHours = project.SecondaryEditorDetails.FinalBillableHours;
                _project.SecondaryEditorDetails.AdjustmentHours = project.SecondaryEditorDetails.AdjustmentHours;
                foreach (var property in typeof(ProjectCalculationDetails).GetProperties())
                {
                    var newValue = property.GetValue(project.CalculationDetails);
                    property.SetValue(_project.CalculationDetails, newValue);
                }
                _project.SubmissionStatus = project.SubmissionStatus;
                context.Projects.Update(_project);
                await context.SaveChangesAsync();
                await _broadcaster.NotifyAllAsync();
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

    public async Task ArchiveProjectAsync(int projectId, string reason)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var project = await context.Projects.AsTracking().FirstOrDefaultAsync(p => p.ProjectId == projectId);
            if (project != null && !project.IsArchived)
            {
                project.IsArchived = true;
                project.InternalOrder = null;
                project.ExternalOrder = null;
                project.Archive = new Archive { ProjectId = projectId, Reason = reason, ArchiveDate = DateTime.UtcNow };
                await context.SaveChangesAsync();
        await _broadcaster.NotifyAllAsync();
            }
        }
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
                        .Max(p => (int?) p.InternalOrder) ?? 0;

                    project.InternalOrder = highestOrder + 1;

                    var highestOrderClient = context.Projects
                        .Where(p => !p.IsArchived && p.ClientId == project.ClientId)
                        .Max(p => (int?) p.ExternalOrder) ?? 0;

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
                var project = await context.Projects
                    .AsTracking()
                    .Where(p => p.ProjectId == _project.ProjectId)
                    .Include(p => p.Client)
                    .Include(p => p.PrimaryEditor)
                    .Include(p => p.SecondaryEditor)
                    .Include(p => p.PrimaryEditorDetails)
                    .Include(p => p.SecondaryEditorDetails)
                    .FirstOrDefaultAsync(p => p.ProjectId == _project.ProjectId);

                // Calculate client billable amount
                if (project.ClientBillableHours != null && project.Client.HourlyRate != null)
                {
                    project.ClientBillableAmount = project.Client.HourlyRate.Value * project.ClientBillableHours;
                }

                // Calculate primary editor details
                if (project.PrimaryEditor != null && project.PrimaryEditorDetails.BillableHours != null)
                {
                    CalculateEditorPayment(project.PrimaryEditorDetails, project.PrimaryEditor, project.ClientBillableHours);
                    context.Entry(project.PrimaryEditorDetails).State = EntityState.Modified;
                }

                // Calculate secondary editor details
                if (project.SecondaryEditor != null && project.SecondaryEditorDetails.BillableHours != null)
                {
                    CalculateEditorPayment(project.SecondaryEditorDetails, project.SecondaryEditor, project.ClientBillableHours);
                    context.Entry(project.SecondaryEditorDetails).State = EntityState.Modified;
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                await _broadcaster.NotifyAllAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Bug in calculating total project price in {nameof(CalculateProjectFinalPrice)}: \n\n {ex} ");
            }
        }
    }

    private void CalculateEditorPayment(EditorDetails editorDetails, ApplicationUser editor, decimal? clientBillableHours)
    {
        editorDetails.Overtime = ( editorDetails.BillableHours ?? 0 ) - ( editorDetails.FinalBillableHours ?? 0 );
        editorDetails.FinalBillableHours = editorDetails.BillableHours -
            ( editorDetails.Overtime ?? 0 ) + ( editorDetails.AdjustmentHours ?? 0 );
        editorDetails.PaymentAmount = ( editor.HourlyRate ?? 0 ) * ( editorDetails.FinalBillableHours ?? 0 );
    }

}