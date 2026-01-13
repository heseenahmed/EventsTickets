using Tickets.Application.Common;

namespace Tickets.API
{
    public static class FileHelperStartup
    {
        public static void Configure(WebApplication app)
        {
            IFileHelper.Configure(
                app.Services.GetRequiredService<IWebHostEnvironment>(),
                app.Services.GetRequiredService<IHttpContextAccessor>());
        }
    }
}
