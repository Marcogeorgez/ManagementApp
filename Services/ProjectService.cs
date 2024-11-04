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
        if (isArchived == false)
        {
            return await _context.Projects
                .Where(p => !p.IsArchived)
                .Include(p => p.VideoEditors)
                .Include(p => p.ClientPayment)
                .Include(p => p.Chats)
                .Include(p => p.Archive)
                .Include(p => p.EditorPayments)
                .ToListAsync();
        }
        else
        {
            return await _context.Projects
                .Include(p => p.VideoEditors)
                .Include(p => p.ClientPayment)
                .Include(p => p.Chats)
                .Include(p => p.Archive)
                .Include(p => p.EditorPayments)
                .ToListAsync();
        }
    }

    public async Task<Project> GetProjectByIdAsync(int projectId)
    {

        var project = await _context.Projects
            .Include(p => p.VideoEditors)
            .Include(p => p.ClientPayment)
            .Include(p => p.Chats)
            .Include(p => p.Archive)
            .Include(p => p.EditorPayments)
            .FirstOrDefaultAsync(p => p.ProjectId == projectId);
        if(project == null)
        {
            return null;
        }
        return project;
    }

    public async Task AddProjectAsync(Project project)
    {
        var x = await _context.Projects.FirstOrDefaultAsync(p => p.ProjectId == project.ProjectId);

        if (x == null)
        {
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            return;
        }
        else
        {
            // TODO: add found project with same ID

            return;
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

