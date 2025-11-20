using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class EmailController : ControllerBase
{
    private readonly EmailService _email;
    public EmailController(EmailService email) => _email = email;

    [HttpPost("SendEmail")]
    public async Task<IActionResult> SendToMe()
    {
        // thay bằng email bạn muốn nhận
        var myEmail = "phuochuynh.03022006@gmail.com";
        await _email.SendEmailAsync(myEmail, "Hơi thở của Nước", "<p>Đây là email từ lab 4 nè</p>");
        return Ok(new { status = "sent" });
    }
}