using FleetLinker.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace FleetLinker.Infra.Configuration
{
    public class BaseEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : BaseEntity
    {
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(e => e.CreatedDate).IsRequired();
            builder.Property(t => t.IsActive).HasColumnType("bit").IsRequired();//.HasDefaultValue(true); 
        }
    }
}
