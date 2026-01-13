using Microsoft.AspNetCore.Http;
namespace FleetLinker.Application.Common
{
    public static class ImageHelper
    {
        public static async Task<string> SaveImageAsync(IFormFile image, string folderPath ,  string baseUrl)
        {
            if (image == null || image.Length == 0)
                throw new ArgumentException("Invalid image file.");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
            var filePath = Path.Combine(folderPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }
            return $"{baseUrl}/uploads/{fileName}";
        }
        public static void DeleteImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return;
            var fileName = Path.GetFileName(new Uri(imageUrl).LocalPath);
            var filePath = Path.Combine("wwwroot", "uploads", fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath); 
            }
        }
    }
}
