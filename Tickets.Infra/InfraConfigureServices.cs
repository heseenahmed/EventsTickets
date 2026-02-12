using Tickets.Application.Common.Caching;
using Tickets.Application.Common.Localization;
using Tickets.Domain.Entity;
using Tickets.Domain.IRepository;
using Tickets.Infra.Caching;
using Tickets.Infra.Data;
using Tickets.Infra.Persistence;
using Tickets.Infra.Repository;
using Tickets.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
namespace Tickets.Infra
{
    public static class InfraConfigureServices
    {
        public static IServiceCollection AddInfraServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            services.Configure<AuthMessageSenderOptions>(configuration.GetSection("EmailSettings"));
            services.Configure<MailSettings>(configuration.GetSection("MailSettings"));
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("AppDBContext"),
                    sqlServerOptionsAction: sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    });
            });
            var redisConnectionString = configuration["Redis:ConnectionString"];

            if (!string.IsNullOrEmpty(redisConnectionString) && (env.IsDevelopment() || redisConnectionString != "localhost:6379"))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                    options.InstanceName = configuration["Redis:InstanceName"] ?? "Tickets:";
                });
            }
            else
            {
                // Fallback to memory cache if Redis is not configured or pointing to localhost in production
                services.AddDistributedMemoryCache();
            }
            services.AddScoped<IEmailSender, MailSender>();  // Register the MailSender service
            services.AddTransient<IMailRepository, MailRepository>();
            services.AddScoped<IServiceProvider, ServiceProvider>();
            services.AddScoped<ITokenRepository, TokenRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICacheService, RedisCacheService>();
            services.AddScoped<INoteRepository, NoteRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<ITicketRepository, TicketRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            return services;
        }
    }
}
