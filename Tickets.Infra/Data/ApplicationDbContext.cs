using Tickets.Domain.Entity;
using Tickets.Infra.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using System.Reflection.Emit;
namespace Tickets.Infra.Data
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

            builder.Entity<Note>(e =>
            {
                e.ToTable("Notes");
                e.HasKey(x => x.Id);
                e.Property(x => x.Title).IsRequired().HasMaxLength(200);
                e.Property(x => x.Details).IsRequired().HasMaxLength(4000);
            });

            builder.Entity<Booking>(e =>
            {
                e.ToTable("Bookings");
                e.HasKey(x => x.Id);
                e.Property(x => x.QrCodeData).IsRequired().HasMaxLength(500);
                e.Property(x => x.TotalPrice).HasColumnType("decimal(18,2)");
                e.Property(x => x.AttendeeName).IsRequired().HasMaxLength(200);
                e.Property(x => x.AttendeeEmail).IsRequired().HasMaxLength(200);
                e.Property(x => x.AttendeePhone).IsRequired().HasMaxLength(250);
                
                e.HasOne(x => x.Student)
                    .WithMany()
                    .HasForeignKey(x => x.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Event)
                    .WithMany()
                    .HasForeignKey(x => x.EventId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Event>(e =>
            {
                e.ToTable("Events");
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).IsRequired().HasMaxLength(200);
                e.Property(x => x.Location).IsRequired().HasMaxLength(500);
                e.Property(x => x.Price).HasColumnType("decimal(18,2)");
                e.Property(x => x.VisitorFee).HasColumnType("decimal(18,2)");
            });

            builder.Entity<Ticket>(e =>
            {
                e.ToTable("Tickets");
                e.HasKey(x => x.Id);
                e.Property(x => x.TotalPrice).HasColumnType("decimal(18,2)");
                e.Property(x => x.AttendeeName).IsRequired().HasMaxLength(200);
                e.Property(x => x.AttendeeEmail).IsRequired().HasMaxLength(200);
                e.Property(x => x.AttendeePhone).IsRequired().HasMaxLength(20);
                e.Property(x => x.QrToken).HasMaxLength(500);
            });

            builder.Entity<Order>(e =>
            {
                e.ToTable("Orders");
                e.HasKey(x => x.Id);
                e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
                e.Property(x => x.Currency).HasMaxLength(10).IsRequired();
                e.Property(x => x.ReferenceType).HasMaxLength(100).IsRequired();
                e.HasIndex(x => x.ReferenceId);
                e.HasMany(x => x.PaymentAttempts).WithOne(x => x.Order).HasForeignKey(x => x.OrderId);
            });

            builder.Entity<PaymentAttempt>(e =>
            {
                e.ToTable("PaymentAttempts");
                e.HasKey(x => x.Id);
                e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
                e.Property(x => x.Currency).HasMaxLength(10).IsRequired();
                e.Property(x => x.IdempotencyKey).HasMaxLength(200);
                e.Property(x => x.PaymobIntentionId).HasMaxLength(200);
                e.Property(x => x.PaymobTransactionId).HasMaxLength(200);
                e.Property(x => x.PaymobOrderReference).HasMaxLength(200);
                e.Property(x => x.MerchantOrderReference).HasMaxLength(200);

                e.HasIndex(x => x.OrderId);
                e.HasIndex(x => x.IdempotencyKey).IsUnique();
                e.HasIndex(x => x.PaymobTransactionId);
                e.HasIndex(x => x.PaymobIntentionId);
                e.HasIndex(x => x.MerchantOrderReference);
            });

            builder.Entity<PaymentWebhookLog>(e =>
            {
                e.ToTable("PaymentWebhookLogs");
                e.HasKey(x => x.Id);
                e.Property(x => x.Payload).IsRequired();
                e.Property(x => x.Provider).HasMaxLength(50).IsRequired();
                e.Property(x => x.ExternalEventId).HasMaxLength(200);
                e.HasIndex(x => x.ExternalEventId);
                e.HasIndex(x => x.PayloadHash);
            });

            foreach (var relationship in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
        public virtual DbSet<Note> Note { get; set; }
        public virtual DbSet<Booking> Bookings { get; set; }
        public virtual DbSet<Event> Events { get; set; }
        public virtual DbSet<Ticket> Tickets { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<PaymentAttempt> PaymentAttempts { get; set; }
        public virtual DbSet<PaymentWebhookLog> PaymentWebhookLogs { get; set; }
    }
}
