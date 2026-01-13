using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace FleetLinker.Domain.IRepository
{
    public  interface IEmailSender
    {
        Task SendEmailAsync(string host, int? port, bool ssl, string emailSender, string password, string email, string subject, string message, string fromName, string fromEmail);
    }
}
