namespace LuminaryVisuals.Services;

using LuminaryVisuals.Data;
using LuminaryVisuals.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ProjectService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(IDbContextFactory<ApplicationDbContext> context, ILogger<ProjectService> logger)
    {
        _contextFactory = context;
        _logger = logger;
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
                .ToListAsync();
            var userIds = projects
                    .SelectMany(p => new[] { p.ClientId, p.PrimaryEditorId, p.SecondaryEditorId })
                    .Where(id => id != null)
                    .Distinct()
                    .ToList();

            // Step 3: Fetch the user names,hourly rate based on the collected IDs
            var userNames = await context.Users
                    .Where(u => userIds.Contains(u.Id))
                    .Select(u => new { u.Id, u.UserName, u.HourlyRate })
                    .ToDictionaryAsync(u => u.Id, u => u.UserName);

            // Step 4: Map user names back to each project
            foreach (var project in projects)
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
                if (project.ClientBillableHours != null && project.Client.HourlyRate != null && project.PrimaryEditor != null)
                {
                    project.PrimaryEditorDetails.Overtime = (project.ClientBillableHours ?? 0)- (project.PrimaryEditorDetails.BillableHours ?? 0 );
                    project.ClientBillableAmount = project.Client.HourlyRate.Value * project.ClientBillableHours;
                    project.PrimaryEditorDetails.PaymentAmount = (project.PrimaryEditor.HourlyRate ?? 0) * (project.PrimaryEditorDetails.BillableHours ?? 0 );
                }

            }
            await context.SaveChangesAsync();
            return projects;
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
    public async Task<List<Project?>> GetProjectsForEditors(bool isArchived, string UserId)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var project = await context.Projects
            .Where(p => p.IsArchived == isArchived && (p.PrimaryEditorId == UserId || p.SecondaryEditorId == UserId))
                .Include(p => p.Archive)
                .Include(p => p.Client)
                .Include(p => p.PrimaryEditor)
                .Include(p => p.SecondaryEditor)
                .Include(p => p.Revisions)
            .ToListAsync();
            if (project == null)
                return null;

            return project;
        }
    }
    public async Task<List<Project?>> GetProjectsForClients(bool isArchived, string UserId)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var project = await context.Projects
            .Where(p => p.IsArchived == isArchived && p.ClientId == UserId)
            .Include(p => p.Chats)
            .Include(p => p.Archive)
            .Include(p => p.Revisions)
            .ToListAsync();
            if (project == null)
                return null;
            return project;
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

    public async Task UpdateProjectAsync(Project project)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var _project = await context.Projects
                .Include(p => p.Revisions)
                .FirstOrDefaultAsync(p => p.ProjectId ==  project.ProjectId);
            if (_project != null)
            {
                if (_project.InternalOrder != project.InternalOrder && _project.InternalOrder != null)
                {
                    await ReorderProjectAsync(project.ProjectId, project.InternalOrder!.Value);
                }
                if (_project.ExternalOrder != project.ExternalOrder && _project.ExternalOrder != null)
                {
                    await ExternalOrderAsync(project.ProjectId, project.ExternalOrder!.Value);
                }
                if(_project.ClientId != project.ClientId)
                {
                    if (project.Client.WeeksToDueDateDefault == null)
                    {
                        project.Client.WeeksToDueDateDefault = 4;
                    }
                }
                context.Entry(_project).CurrentValues.SetValues(project);
                if(project.Revisions != null)
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
                context.Projects.Update(_project);

                await context.SaveChangesAsync();
            }
        }
    }
    public async Task UpdateProjectBillableHoursAsync(Project project)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var _project = await context.Projects
                .Include(p => p.Client)
                .FirstAsync(p => p.ProjectId == project.ProjectId);
            if (_project != null)
            {
                _project.ClientBillableHours = project.ClientBillableHours;
                _project.ClientBillableAmount = (project.Client.HourlyRate ?? 0)* project.ClientBillableHours;
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
            }
        }
    }

    public async Task ReorderProjectAsync(int projectId, int? newOrder)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var project = await context.Projects.AsTracking().FirstOrDefaultAsync(p => p.ProjectId == projectId);
                if (project == null)
                    return;

                // Handle case where the project has been archived (InternalOrder = null)
                if (newOrder == null)
                {
                    // Reorder all non-archived projects starting from 1
                    var projectsToReorder = await context.Projects
                        .AsTracking()
                        .Where(p => p.IsArchived == false)
                        .OrderBy(p => p.InternalOrder)
                        .ToListAsync();

                    int order = 1;  // Start re-ordering from 1
                    foreach (var p in projectsToReorder)
                    {
                        p.InternalOrder = order++;
                    }

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return; // Exit since reordering is complete
                }

                int? oldOrder = project.InternalOrder;
                project.InternalOrder = null;
                if (oldOrder.HasValue)
                {
                    if (newOrder < oldOrder)
                    {
                        // Move upward: Increment other projects between newOrder and oldOrder
                        await context.Projects
                            .AsTracking()
                            .Where(p => p.InternalOrder >= newOrder && p.InternalOrder < oldOrder && p.IsArchived == false)
                            .ForEachAsync(p => p.InternalOrder++);
                    }
                    else if (newOrder > oldOrder)
                    {
                        // Move downward: Decrement other projects between oldOrder and newOrder
                        await context.Projects
                            .AsTracking()
                            .Where(p => p.InternalOrder > oldOrder && p.InternalOrder <= newOrder && p.IsArchived == false)
                            .ForEachAsync(p => p.InternalOrder--);
                    }
                }

                // Assign the new order to the current project
                project.InternalOrder = newOrder;
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                // Rollback the transaction in case of an error
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
    public async Task ExternalOrderAsync(int projectId, int? newOrder)
    {
        using (var context = _contextFactory.CreateDbContext())
        {

            var project = await context.Projects.AsTracking().FirstOrDefaultAsync(p => p.ProjectId == projectId);
            var clientId = project.ClientId;
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {

                if (newOrder == null)
                {
                    // Reorder all non-archived projects for the client
                    var projectsToReorder = await context.Projects
                        .AsTracking()
                        .Where(p => p.ClientId == clientId && p.IsArchived == false)
                        .OrderBy(p => p.ExternalOrder)
                        .ToListAsync();

                    int order = 1;
                    foreach (var p in projectsToReorder)
                    {
                        p.ExternalOrder = order++;
                    }

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return;
                }

                int? oldOrder = project.ExternalOrder;
                project.ExternalOrder = null;
                if (oldOrder.HasValue)
                {
                    if (newOrder < oldOrder)
                    {
                        await context.Projects
                            .AsTracking()
                            .Where(p => p.ClientId == clientId && p.ExternalOrder >= newOrder && p.ExternalOrder < oldOrder && p.IsArchived == false)
                            .ForEachAsync(p => p.ExternalOrder++);
                    }
                    else if (newOrder > oldOrder)
                    {
                        await context.Projects
                            .AsTracking()
                            .Where(p => p.ClientId == clientId && p.ExternalOrder > oldOrder && p.ExternalOrder <= newOrder && p.IsArchived == false)
                            .ForEachAsync(p => p.ExternalOrder--);
                    }
                }

                project.ExternalOrder = newOrder;
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }

            catch
            {
                // Rollback the transaction in case of an error
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}