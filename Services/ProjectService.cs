namespace LuminaryVisuals.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LuminaryVisuals.Data.Entities;
using LuminaryVisuals.Data;
using Microsoft.AspNetCore.Http.HttpResults;

public class ProjectService
{
    private readonly ApplicationDbContext _context;

    public ProjectService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Project>> GetProjectsAsync(bool isArchived)
    {
            return await _context.Projects
                .Where(p => p.IsArchived == isArchived)
                .Include(p => p.VideoEditors)
                .Include(p => p.ClientPayment)
                .Include(p => p.Chats)
                .Include(p => p.Archive)
                .Include(p => p.EditorPayments)
                .ToListAsync();
    }

    public async Task<Project?> GetProjectByCLientIdAsync(bool isArchived,string UserId)
    {

        var project = await _context.Projects
            .Where(p => p.IsArchived == isArchived)
            .Include(p => p.VideoEditors)
            .Include(p => p.ClientPayment)
            .Include(p => p.Chats)
            .Include(p => p.Archive)
            .Include(p => p.EditorPayments)
            .FirstOrDefaultAsync(p => p.ClientId == UserId);
            if (project == null)
                return null;
            return project;      
    }
/*    public async Task<Project?> GetProjectByEditorIdAsync(bool isArchived, string EditorId)
    {

        var project = await _context.Projects
            .Where(p => p.IsArchived == isArchived)
            .Include(p => p.VideoEditors)
            .Include(p => p.ClientPayment)
            .Include(p => p.Chats)
            .Include(p => p.Archive)
            .Include(p => p.EditorPayments)
            .FirstOrDefaultAsync(p => p. == EditorId);
        if (project == null)
            return null;
        return project;
    }*/

    public async Task AddProjectAsync(Project project)
    {
        var client = await _context.Users.FirstOrDefaultAsync(u => u.Id == project.ClientId);

        if (client == null)
        {
            // Handle the case where the client does not exist.
            throw new Exception("Client not found.");
        }

        // If the project doesn't already exist, add it
        var existingProject = await _context.Projects.FirstOrDefaultAsync(p => p.ProjectId == project.ProjectId);

        if (existingProject == null)
        {
            // Set the ClientId for the project, though it's already set in the project, it's good to confirm
            project.ClientId = client.Id;  // Ensure ClientId is assigned to the project

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

    public async Task UpdateProjectAsync(Project project)
    {        
        _context.Projects.Update(project);
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

