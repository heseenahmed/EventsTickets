using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
namespace FleetLinker.Application.DTOs
{
    public class ExcelDto
    {
        [Required]
        public IFormFile File { get; set; }
    }
}
