using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Text.Json.Serialization;
using FleetLinker.Domain.Enums;
namespace FleetLinker.Domain.Entity
{
    public class ApplicationUser : IdentityUser
    {
        [Display(Name = "FullName")]
        public string FullName { get; set; } = null!;
        [Display(Name = "National ID")]
        public string? SSN { get; set; }
        public DeviceType DeviceType { get; set; }
        [Display(Name = "Refresh Token")]
        public string? RefreshToken { get; set; }
        [Display(Name = "Refresh Token Expiry UTC")]
        public DateTime? RefreshTokenExpiryUTC { get; set; }
        [Display(Name = "Deleted")]
        [DefaultValue(false)]
        public bool IsDeleted { get; set; }
        [Display(Name = "Active")]
        public bool IsActive { get; set; }
        [Display(Name = "Birth Day")]
        public DateTime BirthDay { get; set; }
        [DefaultValue(true)]
        public bool FirstTimeLogin { get; set; }
        [DefaultValue(false)]
        public bool IsOTPEnabled { get; set; }
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
    }
    public class ApplicationUserRole : IdentityUserRole<string>
    {
        public virtual ApplicationUser User { get; set; }
        public virtual ApplicationRole Role { get; set; }
    }
    public class ApplicationRole : IdentityRole<string>
    {
        public ApplicationRole() { }
        public ApplicationRole(string roleName)
            : base(roleName)
        {
        }
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
    }
}
