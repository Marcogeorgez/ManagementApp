using LuminaryVisuals.Data;
using LuminaryVisuals.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace LuminaryVisuals.Controller;
[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly ILogger<MessagesController> logger;
    private readonly IDbContextFactory<ApplicationDbContext> contextFactory;

    public MessagesController(ILogger<MessagesController> logger, IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        this.logger = logger;
        this.contextFactory = contextFactory;
    }

    [HttpPost("subscribe")]
    [Authorize]
    public async Task<IActionResult> Subscribe([FromBody] PushSubscriptionModel subscription)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        try
        {
            using var context = contextFactory.CreateDbContext();
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return Unauthorized();
            // Check if the user already has a subscription
            var existingSubscription = await context.PushNotificationSubscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Endpoint == subscription.Endpoint && s.P256DH == subscription.Keys.P256DH);

            if (existingSubscription == null)
            {
                existingSubscription = new PushNotificationSubscriptions
                {
                    UserId = user.Id,
                    Endpoint = subscription.Endpoint,
                    P256DH = subscription.Keys.P256DH,
                    Auth = subscription.Keys.Auth
                };
            }
            else
            {
                logger.LogInformation($"User {user.Id} already subscribed to endpoint {subscription.Endpoint}.");
                return Ok("Already subscribed with this device.");
            }
            await context.PushNotificationSubscriptions.AddAsync(existingSubscription);
            await context.SaveChangesAsync();
            return Ok(new { message = "Subscribed successfully!" });
        }
        catch (Exception)
        {

            throw;
        }
    }
}

public class PushSubscriptionModel
{
    [Required]
    public string Endpoint { get; set; }

    [Required]
    public Keys Keys { get; set; }
}

public class Keys
{
    [Required]
    public string P256DH { get; set; }

    [Required]
    public string Auth { get; set; }
}

