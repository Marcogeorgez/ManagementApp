using LuminaryVisuals.Data;
using LuminaryVisuals.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LuminaryVisuals.Services.Core
{
    public class LoggingHours
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<ProjectService> _logger;

        public LoggingHours(IDbContextFactory<ApplicationDbContext> context, ILogger<ProjectService> logger)
        {
            _contextFactory = context;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new EditorLoggingHours entry.
        /// </summary>
        public async Task<bool> CreateAsync(EditorLoggingHours loggingHours)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                {
                    loggingHours.Date = loggingHours.Date.Date;
                    context.EditorLoggingHours.Add(loggingHours);
                    await context.SaveChangesAsync();
                    await CalculateAndSaveBillableHoursAsync(loggingHours);
                    _logger.LogInformation("LoggingHours entry created successfully.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating LoggingHours entry.");
                return false;
            }
        }

        /// <summary>
        /// Updates an existing EditorLoggingHours entry.
        /// </summary>
        public async Task<bool> UpdateAsync(EditorLoggingHours updatedLoggingHours)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                {
                    var existing = await context.EditorLoggingHours
                        .FirstOrDefaultAsync(e => e.Id == updatedLoggingHours.Id);

                    if (existing == null)
                    {
                        _logger.LogWarning("LoggingHours entry not found for ID: {Id}", updatedLoggingHours.Id);
                        return false;
                    }

                    existing.UserId = updatedLoggingHours.UserId;
                    existing.ProjectId = updatedLoggingHours.ProjectId;
                    existing.Date = updatedLoggingHours.Date.Date; // Ensure time is truncated
                    existing.EditorWorkingHours = updatedLoggingHours.EditorWorkingHours;

                    context.EditorLoggingHours.Update(existing);
                    await context.SaveChangesAsync();
                    await CalculateAndSaveBillableHoursAsync(updatedLoggingHours);
                    _logger.LogInformation("LoggingHours entry updated successfully.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating LoggingHours entry.");
                throw;
            }
        }

        /// <summary>
        /// Gets a list of EditorLoggingHours entries specified by project.
        /// </summary>
        public async Task<List<EditorLoggingHours>> GetAllAsync(int projectId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                {
                    return await context.EditorLoggingHours
                        .Include(e => e.User)
                        .Include(e => e.Project)
                        .Where(e => e.ProjectId == projectId)
                        .ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving LoggingHours for projectId {projectId}.");
                return new List<EditorLoggingHours>();
            }
        }

        /// <summary>
        /// Gets a specific EditorLoggingHours entry by ProjectID and userId.
        /// </summary>
        public async Task<List<EditorLoggingHours?>?> GetByIdAsync(int projectId, string currentUserId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                {
                    var loggedHours = await context.EditorLoggingHours
                        .Include(e => e.User)
                        .Include(e => e.Project)
                        .Where(e => e.ProjectId == projectId && e.UserId == currentUserId)
                        .ToListAsync();
                    if (loggedHours != null)
                        return loggedHours!;
                    else
                        return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving LoggingHours entry for ID: {Id}", projectId);
                return null;
            }
        }
        public async Task<bool> DeleteLoggedHoursAsync(int projectId, string currentUserId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                {
                    // Retrieve all logged hours for the specified project and user
                    var loggedHours = await context.EditorLoggingHours
                        .AsTracking()
                        .Where(e => e.ProjectId == projectId && e.UserId == currentUserId)
                        .ToListAsync();

                    if (loggedHours != null && loggedHours.Any())
                    {
                        // Remove the logged hours from the context
                        context.EditorLoggingHours.RemoveRange(loggedHours);

                        await context.SaveChangesAsync();

                        return true; // Deletion successful
                    }

                    return false; // No entries to delete
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting LoggingHours entries for ProjectId: {projectId}, UserId: {currentUserId}");
                return false; // Indicate failure
            }
        }

        public async Task<bool> CalculateAndSaveBillableHoursAsync(EditorLoggingHours EditorLoggingHours)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                // Fetch all EditorLoggingHours
                var loggedHours = await context.EditorLoggingHours
                    .Where(e => e.ProjectId == EditorLoggingHours.ProjectId && e.UserId == EditorLoggingHours.UserId)
                    .SumAsync(e => e.EditorWorkingHours ?? 0);


                // Fetch projects and update their BillableHours
                var project = await context.Projects
                    .AsTracking()
                    .Include(p => p.PrimaryEditor)
                    .Include(p => p.SecondaryEditor)
                    .FirstOrDefaultAsync(p => p.ProjectId == EditorLoggingHours.ProjectId);
                if (project == null)
                {
                    throw new Exception("Project not found.");
                }
                if (project.PrimaryEditor?.Id == EditorLoggingHours.UserId)
                {
                    project.PrimaryEditorDetails.BillableHours = loggedHours;

                }
                else if (project.SecondaryEditor?.Id == EditorLoggingHours.UserId)
                {
                    project.SecondaryEditorDetails.BillableHours = loggedHours;
                }
                else
                {
                    throw new Exception("CalculateAndSaveBillableHoursAsync can't find PrimaryeditorId or SecondaryEditorId");
                }

                // Save changes to the database
                await context.SaveChangesAsync();
                _logger.LogInformation("BillableHours updated successfully for all projects.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error calculating and saving billable hours.", ex);
                return false;
            }
        }

    }
}
