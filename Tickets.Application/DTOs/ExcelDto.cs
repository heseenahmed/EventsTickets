using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
namespace Tickets.Application.DTOs
{
    public class ExcelDto
    {
        [Required]
        public IFormFile File { get; set; }
    }
}
