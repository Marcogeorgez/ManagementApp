namespace LuminaryVisuals.Services;

using LuminaryVisuals.Data;
using LuminaryVisuals.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class ProjectService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(ApplicationDbContext context, ILogger<ProjectService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Project>> GetProjectsAsync(bool isArchived)
    {
        var projects = await _context.Projects
                .Where(p => p.IsArchived == isArchived)
                .ToListAsync();
        var userIds = projects
                .SelectMany(p => new[] { p.ClientId, p.PrimaryEditorId, p.SecondaryEditorId })
                .Where(id => id != null)
                .Distinct()
                .ToList();

        // Step 3: Fetch the user names based on the collected IDs
        var userNames = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName })
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
        }

        return projects;
    }

    public async Task<List<Project>> GetProjectsAsync()
    {
        return await _context.Projects
            .ToListAsync();
    }

    public async Task<List<Project?>> GetProjectByClientIdAsync(bool isArchived, string UserId)
    {
        var project = await _context.Projects
            .Where(p => p.IsArchived == isArchived && p.ClientId == UserId)
            .Include(p => p.ClientPayment)
            .Include(p => p.Chats)
            .Include(p => p.Archive)
            .ToListAsync();
        if (project == null)
            return null;
        return project;
    }

    public async Task AddProjectAsync(Project project)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == project.ClientId);

        if (user == null)
        {
            // Handle the case where the client does not exist.
            throw new Exception("Client not found.");
        }

        // If the project doesn't already exist, add it
        var existingProject = await _context.Projects.FirstOrDefaultAsync(p => p.ProjectId == project.ProjectId);

        if (existingProject == null)
        {
            // Set the ClientId for the project, though it's already set in the project, it's good to confirm
            project.ClientId = user.Id;  // Ensure ClientId is assigned to the project

            // Add the project to the context and save changes
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
        }
        else
        {
            // Handle the case where the project already exists (optional)
            throw new Exception("Project already exists.");
        }
    }

    public async Task AssignProjectToClientAsync(int projectId, string newUserId)
    {
        // Make sure the project exists in the database
        var _project = await _context.Projects.FindAsync(projectId);

        // Ensure the new client (user) exists
        var client = await _context.Users.FirstOrDefaultAsync(u => u.Id == newUserId);

        if (client != null && _project != null)
        {
            // Update the ClientId property to the new client's ID
            _project.ClientId = client.Id;
            // Currently when the project is assigned to a new user. It updates the shoot date by the weeks to due date default from THE DATE OF SHOOT DATE.
            _project.DueDate = _project.ShootDate.Value.AddDays((double) client.WeeksToDueDateDefault! * 7);
            await _context.SaveChangesAsync();
        }
        else
        {
            throw new ArgumentException("Project or client not found.");
        }
    }

    public async Task AssignProjectToPrimaryEditorAsync(int projectId, string userId)
    {
        var _project = await _context.Projects.FindAsync(projectId);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user != null && _project != null)
        {
            _project.PrimaryEditorId = userId;
            _context.Projects.Update(_project);
            await _context.SaveChangesAsync();
        }
        else
        {
            throw new ArgumentException("Project/User not found");
        }
    }

    public async Task AssignProjectToSecondaryEditorAsync(string userId, string projectId)
    {
        var _project = await _context.Projects.FindAsync(projectId);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user != null && _project != null)
        {
            _project.SecondaryEditorId = userId;
            _context.Projects.Update(_project);
            await _context.SaveChangesAsync();
        }
        else
        {
            throw new ArgumentException("Project/User not found");
        }
    }


    public async Task UpdateProjectAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteProjectAsync(int projectId)
    {
        var project = await _context.Projects.FindAsync(projectId);
        if (project.ProjectId == projectId)
        {
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ArchiveProjectAsync(int projectId, string reason)
    {
        var project = await _context.Projects.FindAsync(projectId);
        if (project != null && !project.IsArchived)
        {
            project.IsArchived = true;
            project.Archive = new Archive { ProjectId = projectId, Reason = reason, ArchiveDate = DateTime.UtcNow };
            await _context.SaveChangesAsync();
        }
    }

    public async Task UnarchiveProjectAsync(int projectId)
    {
        var project = await _context.Projects.FindAsync(projectId);
        if (project != null && project.IsArchived)
        {
            await DeleteArchiveAsync(projectId);
            project.IsArchived = false;
            await _context.SaveChangesAsync();
        }
        else
        {
            throw new ArgumentException("Project or client not found.");
        }
    }

    public async Task DeleteArchiveAsync(int projectId)
    {
        var archiveRecord = await _context.Archives
            .FirstOrDefaultAsync(a => a.ProjectId == projectId);

        if (archiveRecord != null)
        {
            _context.Archives.Remove(archiveRecord);
            await _context.SaveChangesAsync();
        }
    }
}