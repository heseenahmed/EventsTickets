using FleetLinker.Application.Common;

namespace FleetLinker.API
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
