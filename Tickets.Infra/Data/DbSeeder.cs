using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Tickets.Domain.Entity;

namespace Tickets.Infra.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            var roles = new List<(string Id, string Name)>
            {
                ("11111111-1111-1111-1111-111111111111", "Admin"),
                ("22222222-2222-2222-2222-222222222222", "EventOwner")
            };

            foreach (var (id, name) in roles)
            {
                var roleExists = await roleManager.RoleExistsAsync(name);
                if (!roleExists)
                {
                    var role = new ApplicationRole(name)
                    {
                        Id = id,
                        NormalizedName = name.ToUpperInvariant()
                    };
                    await roleManager.CreateAsync(role);
                }
            }
        }
    }
}
