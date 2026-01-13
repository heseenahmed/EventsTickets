using Microsoft.AspNetCore.Http;
namespace FleetLinker.Domain.IRepository
{
    public interface IMailRepository
    {
        Task SendEmailAsync(string mailTo, string subject, string body, IList<IFormFile> attachments = null);
    }
}
