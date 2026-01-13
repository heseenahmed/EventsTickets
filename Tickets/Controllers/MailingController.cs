using Tickets.Domain.IRepository;
using Microsoft.AspNetCore.Mvc;
using SendEmailsWithDotNet5.Dtos;
namespace SendEmailsWithDotNet5.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailingController : ControllerBase
    {
        private readonly IMailRepository _mailingService;
        public MailingController(IMailRepository mailingService)
        {
            _mailingService = mailingService;
        }
        [HttpPost("send")]
        public async Task<IActionResult> SendMail([FromForm] MailRequestDto dto)
        {
            await _mailingService.SendEmailAsync(dto.ToEmail, dto.Subject, dto.Body, dto.Attachments);
            return Ok();
        }
        [HttpPost("welcome")]
        public async Task<IActionResult> SendWelcomeEmail([FromBody] WelcomeRequestDto dto)
        {
            var filePath = $"{Directory.GetCurrentDirectory()}\\Template\\EmailTemplate.html";
            var str = new StreamReader(filePath);
            var mailText = str.ReadToEnd();
            str.Close();
            mailText = mailText.Replace("[username]", dto.UserName).Replace("[email]", dto.Email);
            await _mailingService.SendEmailAsync(dto.Email, "Welcome to our channel", mailText);
            return Ok();
        }
    }
}
