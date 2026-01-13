using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Net;
namespace Tickets.Application.Common
{
    public static class IFileHelper
    {
        private static IWebHostEnvironment _webHostEnvironment;
        private static IHttpContextAccessor _httpContextAccessor;
        public static void Configure(IWebHostEnvironment webHostEnvironment , IHttpContextAccessor httpContextAccessor)
        {
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }
        public static async Task<List<string>> SaveFilesAsync(List<IFormFile>? files, string uploadFolderPath, string baseUrl)
        {
            var fileUrls = new List<string>();
            if (files == null || files.Count == 0)
                return fileUrls;
            if (!Directory.Exists(uploadFolderPath))
                Directory.CreateDirectory(uploadFolderPath);
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    string uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    string filePath = Path.Combine(uploadFolderPath, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    fileUrls.Add($"{baseUrl}/uploads/{uniqueFileName}");
                }
            }
            return fileUrls;
        }
        public static void DeleteFiles(List<string> fileUrls)
        {
            if (fileUrls == null || fileUrls.Count == 0)
                return;
            string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            foreach (var fileUrl in fileUrls)
            {
                var fileName = Path.GetFileName(new Uri(fileUrl).AbsolutePath); // Extract filename from URL
                string filePath = Path.Combine(uploadPath, fileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }
    }
}
