using FleetLinker.Application.Common.Caching;
using FleetLinker.Application.Common.Localization;
using FleetLinker.Domain.Entity;
using FleetLinker.Domain.IRepository;
using FleetLinker.Infra.Caching;
using FleetLinker.Infra.Data;
using FleetLinker.Infra.Persistence;
using FleetLinker.Infra.Repository;
using FleetLinker.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
namespace FleetLinker.Infra
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
                options.UseSqlServer(configuration.GetConnectionString("AppDBContext"));
                Console.WriteLine($"Connection String: {options.UseSqlServer(configuration.GetConnectionString("AppDBContext"))}");
            });
            var redisConnectionString = configuration["Redis:ConnectionString"];

            if (!string.IsNullOrEmpty(redisConnectionString) && (env.IsDevelopment() || redisConnectionString != "localhost:6379"))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                    options.InstanceName = configuration["Redis:InstanceName"] ?? "FleetLinker:";
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
            services.AddScoped<IAdelTokenRepository, AdelTokenRepository>();
            services.AddScoped<INoteRepository, NoteRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            return services;
        }
    }
}