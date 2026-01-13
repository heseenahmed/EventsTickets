using Tickets.Application.Common.Mappings;
using Tickets.Infra.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
namespace Tickets.Application
{
    public static class AppConfigureServices
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(typeof(AppConfigureServices).Assembly, includeInternalTypes: true);

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            services.AddAutoMapper(typeof(MappingProfile));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
            return services;
        }
    }
}
