using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace Backend_API.Controllers
{
    [Route("api/file")]
    [ApiController]
    public class FileController : ControllerBase
    {
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Check file type (accept only png, jpg, and jpeg)
            string[] allowedFileTypes = { "image/png", "image/jpeg", "image/jpg" };
            if (!allowedFileTypes.Contains(file.ContentType.ToLower()))
            {
                return BadRequest("Invalid file type. Only PNG, JPG, and JPEG files are allowed.");
            }

            // Check file size (limit to 3MB)
            long fileSizeLimit = 3 * 1024 * 1024; // 3MB in bytes
            if (file.Length > fileSizeLimit)
            {
                return BadRequest("File size exceeds the limit (3MB).");
            }

            // Ensure the directory exists
            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Generate a unique filename
            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(uploadPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { filename = uniqueFileName });
        }

        [HttpPost("upload-to-cloudinary")]
        public async Task<IActionResult> UploadToCloudinary(IFormFile file, [FromServices] Cloudinary cloudinary)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Check file type (accept only png, jpg, and jpeg)
            string[] allowedFileTypes = { "image/png", "image/jpeg", "image/jpg" };
            if (!allowedFileTypes.Contains(file.ContentType.ToLower()))
            {
                return BadRequest("Invalid file type. Only PNG, JPG, and JPEG files are allowed.");
            }

            // Check file size (limit to 3MB)
            long fileSizeLimit = 3 * 1024 * 1024; // 3MB in bytes
            if (file.Length > fileSizeLimit)
            {
                return BadRequest("File size exceeds the limit (3MB).");
            }

            // Upload the file to Cloudinary
            using (var stream = file.OpenReadStream())
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "eproject-sem3"
                };

                var uploadResult = await cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    return BadRequest("Error uploading the file to Cloudinary.");
                }

                // Return the URL of the uploaded image
                return Ok(new { ImageUrl = uploadResult.SecureUri.AbsoluteUri });
            }
        }

        [HttpGet("images/{imageName}")]
        public IActionResult GetImage(string imageName)
        {
            // Check if the image exists in the "Uploads" folder
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", imageName);
            if (!System.IO.File.Exists(imagePath))
            {
                return NotFound("Image not found");
            }

            // Determine the content type based on the file extension
            string contentType;
            switch (Path.GetExtension(imageName).ToLower())
            {
                case ".png":
                    contentType = "image/png";
                    break;
                case ".jpg":
                case ".jpeg":
                    contentType = "image/jpeg";
                    break;
                // Add more cases for other image formats if needed
                default:
                    contentType = "application/octet-stream"; // Fallback to binary data
                    break;
            }

            // Read the image file and send it as a response with the determined content type
            var imageStream = System.IO.File.OpenRead(imagePath);
            return File(imageStream, contentType);
        }

        [HttpDelete("delete-from-cloudinary")]
        public async Task<IActionResult> DeleteFromCloudinary(string imageUrl, [FromServices] Cloudinary cloudinary)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return BadRequest("Image URL is required.");
            }

            // Extract the public ID from the Cloudinary URL
            string publicId = ExtractPublicIdFromCloudinaryUrl(imageUrl);

            // Delete the image from Cloudinary
            var deleteParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image
            };

            var result = await cloudinary.DestroyAsync(deleteParams);

            if (result.Error != null)
            {
                return BadRequest("Error deleting the image from Cloudinary.");
            }

            return Ok("Image deleted from Cloudinary.");
        }

        private string ExtractPublicIdFromCloudinaryUrl(string imageUrl)
        {
            // Split the URL by '/'
            var parts = imageUrl.Split('/');

            // The public ID is usually the last two parts
            if (parts.Length >= 2)
            {
                // Get the last part (filename)
                var filename = parts[parts.Length - 1];

                // Remove the file extension (e.g., '.jpg') if it exists
                var publicId = filename.Contains('.') ? filename.Substring(0, filename.LastIndexOf('.')) : filename;

                // Combine the last two parts to form the public ID
                return string.Join("/", parts[parts.Length - 2], publicId);
            }

            // If the URL doesn't have the expected structure, return an empty string or handle the error accordingly
            return string.Empty;
        }
    }
}
