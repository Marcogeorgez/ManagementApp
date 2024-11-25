using LuminaryVisuals.Data;
using LuminaryVisuals.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace LuminaryVisuals.Services;


public class UserNoteService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public UserNoteService(IDbContextFactory<ApplicationDbContext> context)
    {
        _contextFactory = context;
    }

    public async Task<MessageSuccess> AddNoteAsync(string targetId, string note)
    {
        if (string.IsNullOrEmpty(targetId))
        {
            return new MessageSuccess { Success = false, Message = "Target ID cannot be null or empty." };
        }
        try
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var userNote = new UserNote
                {
                    TargetUserId = targetId,
                    Note = note
                };

                context.UserNote.Add(userNote);
                await context.SaveChangesAsync();
                return new MessageSuccess { Success = true, Message = "Note has been created successfully" };
            }
        }
        catch (Exception ex)
        {
            return new MessageSuccess { Success = false, Message = $"An error occurred: {ex.Message}" };
        }

    }

    public async Task<IEnumerable<UserNote>> GetAllNotes()
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            return await context.UserNote.ToListAsync();
        }
    }
    public async Task<string> GetNoteByUserId(string userId)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var EntityNote = await context.UserNote.FirstOrDefaultAsync(u => u.TargetUserId == userId);
            if(EntityNote != null )
                return EntityNote.Note;
            else
                return "This user has no notes assigned.";
        }
    }
    public async Task<MessageSuccess> UpdateNoteAsync(int noteId, string updatedNote)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var note = await context.UserNote.AsTracking().FirstOrDefaultAsync(n => n.Id == noteId);
            if (note != null)
            {
                note.Note = updatedNote;
                context.Update(note);
                await context.SaveChangesAsync();
                return new MessageSuccess { Success = true, Message = "Note has been updated successfully" };
            }
            else
            {
                return new MessageSuccess { Success = false, Message = "Note couldn't be updated." };
            }
        }
    }
}
