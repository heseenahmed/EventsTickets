using DocumentFormat.OpenXml.Math;
using FleetLinker.Domain.Entity;
using FleetLinker.Domain.Entity;
using FleetLinker.Infra.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using System.Reflection.Emit;
namespace FleetLinker.Infra.Data
{
    public class ApplicationDbContext : IdentityDbContext<
        ApplicationUser, ApplicationRole, string,
        IdentityUserClaim<string>, ApplicationUserRole, IdentityUserLogin<string>,
        IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            builder.Entity<ApplicationUserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();

            builder.Entity<ApplicationUserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();
            builder.Entity<Tokens>(e =>
            {
                e.ToTable("Tokens");
                e.HasKey(x => x.Id);
                e.Property(x => x.Token).IsRequired().HasMaxLength(500);
            });

            builder.Entity<Note>(e =>
            {
                e.ToTable("Notes");
                e.HasKey(x => x.Id);
                e.Property(x => x.Title).IsRequired().HasMaxLength(200);
                e.Property(x => x.Details).IsRequired().HasMaxLength(4000);
            });
            foreach (var relationship in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
        public virtual DbSet<Tokens> Tokens { get; set; }
        public virtual DbSet<Note> Note { get; set; }
    }
}