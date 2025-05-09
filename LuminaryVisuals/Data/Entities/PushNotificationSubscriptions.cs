namespace LuminaryVisuals.Data.Entities;

public class PushNotificationSubscriptions
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public required string Endpoint { get; set; }
    public required string P256DH { get; set; }
    public required string Auth { get; set; }
    public bool Status { get; set; } = true;
    public ApplicationUser User { get; set; }
}
