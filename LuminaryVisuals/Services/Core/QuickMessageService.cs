using LuminaryVisuals.Data;
using LuminaryVisuals.Data.Entities;
using LuminaryVisuals.Services;
using LuminaryVisuals.Services.Events;
using LuminaryVisuals.Services.Mail;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

public class QuickMessageService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ILogger<QuickMessageService> _logger;

    public QuickMessageService(IDbContextFactory<ApplicationDbContext> contextFactory, ILogger<QuickMessageService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all quick messages from the database.
    /// </summary>
    /// <returns>A collection of QuickMessage objects. If an error occurs during retrieval, the exception is logged and rethrown.</returns>
    /// <exception cref="Exception">Thrown when database access fails.</exception>
    public async Task<List<QuickMessage>> GetAllMessagesAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        try
        {
            return   await context.QuickMessages.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all quick messages");
            throw;
        }
    }
    /// <summary>
    /// Creates and saves a new quick message to the database.
    /// </summary>
    /// <param name="content">The text content of the quick message</param>
    /// <param name="isApproved">Boolean indicating whether the message is pre-approved</param>
    /// <returns>A Task representing the asynchronous operation</returns>
    /// <exception cref="Exception">Thrown when message creation or database save fails</exception>
    public async Task<QuickMessage> CreateMessageAsync(QuickMessage message)
    {
        using var context = _contextFactory.CreateDbContext();
        try
        {
            context.QuickMessages.Add(message);
            await context.SaveChangesAsync();
            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating quick message");
            throw;
        }
    }
    
    /// <summary>
    /// Deletes a quick message by id
    /// </summary>
    /// <param name="id">Id of the message.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task DeleteMessageAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        try
        {
            var message = await context.QuickMessages
                .AsTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
            if (message == null)
            {
                throw new ArgumentException("Message not found");
            }
            context.QuickMessages.Remove(message);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting quick message");
            throw;
        }
    }
    /// <summary>
    /// Updates the content of quick message
    /// </summary>
    /// <param name="id">Id of the message</param>
    /// <param name="content">The updated text content of the message.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"> thrown when message not found.</exception>
    public async Task UpdateMessageAsync(int id, string content)
    {
        using var context = _contextFactory.CreateDbContext();
        try
        {
            var message = await context.QuickMessages
                .AsTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
            if (message == null)
            {
                throw new ArgumentException("Message not found");
            }
            message.Content = content;
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating quick message");
            throw;
        }
    }
}
