namespace LuminaryVisuals.Data.Entities;

public class UserNote
{
    public int Id { get; set; }
    public string TargetUserId { get; set; }
    public string CreatedByUserId { get; set; }
    public string Note { get; set; }

    public virtual ApplicationUser TargetUser { get; set; } 
    public virtual ApplicationUser CreatedByUser { get; set; }
}