using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
namespace FleetLinker.Domain.Entity
{
    public class BaseEntity
    {
        [Key]
        public Guid Id { get; set; } = new Guid();
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
