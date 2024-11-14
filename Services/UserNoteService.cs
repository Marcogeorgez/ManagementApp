using LuminaryVisuals.Data;
using LuminaryVisuals.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LuminaryVisuals.Services;


public class UserNoteService
{
    private readonly ApplicationDbContext _context;

    public UserNoteService(ApplicationDbContext context)
    {
         _context = context;
    }

    public async Task<MessageSuccess> AddNoteAsync(string targetId, string note)
    {
        if (string.IsNullOrEmpty(targetId))
        {
            return new MessageSuccess { Success = false, Message = "Target ID cannot be null or empty." };
        }

        try
        {

            var userNote = new UserNote
            {
                TargetUserId = targetId,
                Note = note
            };

            _context.UserNote.Add(userNote);
            await _context.SaveChangesAsync();
            return new MessageSuccess { Success = true, Message = "Note has been created successfully" };
        }
        catch(Exception ex)
        {
            return new MessageSuccess { Success = false, Message = $"An error occurred: {ex.Message}" };
        }

    }

    public async Task<IEnumerable<UserNote>> GetAllNotes()
    {
        return await _context.UserNote.ToListAsync();
    }

    public async Task<MessageSuccess> UpdateNoteAsync(int noteId, string updatedNote)
    {
        var note = await _context.UserNote.FindAsync(noteId);
        if (note != null)
        {
            _context.Update(note);
            await _context.SaveChangesAsync();
            return new MessageSuccess { Success = true , Message = "Note has been updated successfully"};
        }
        else
        {
            return new MessageSuccess { Success = false, Message = "Note couldn't be updated." };
        }
    }
}
