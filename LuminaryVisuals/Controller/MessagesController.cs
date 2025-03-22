using LuminaryVisuals.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace LuminaryVisuals.Controller;
[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly ChatService _chatService;
    private readonly UserManager<ApplicationUser> _userManager;

    public MessagesController(ChatService chatService, UserManager<ApplicationUser> userManager)
    {
        _chatService = chatService;
        _userManager = userManager;
    }

    [HttpGet("unread")]
    [Authorize] // This will use your existing cookie authentication
    public async Task<IActionResult> GetUnreadCount()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var count = await _chatService.GetUnreadMessageCount(user.Id);
        return Ok(new { count = count, hasNewMessages = count > 0 });
    }
    [HttpPost("subscribe")]
    public IActionResult Subscribe([FromBody] PushSubscriptionModel subscription)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Save to DB or cache (implementation depends on your app)
        return Ok(new { message = "Subscribed successfully!" });
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

