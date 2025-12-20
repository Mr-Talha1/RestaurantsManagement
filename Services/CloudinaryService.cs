using Microsoft.Extensions.Options;
using System.Security.AccessControl;
using System.Security.Principal;

using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using ResourceType = CloudinaryDotNet.Actions.ResourceType;
namespace BIPL_RAASTP2M.Services
{
    public class CloudinarySettings
    {
        public string CloudName { get; set; } = null!;
        public string ApiKey { get; set; } = null!;
        public string ApiSecret { get; set; } = null!;
    }
    public interface ICloudinaryService
    {
        Task<(bool Success, string Url, string PublicId, string Error)> UploadImageAsync(IFormFile file, string folder = "products");
        Task<bool> DeleteImageAsync(string publicId);
    }
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        public CloudinaryService(IOptions<CloudinarySettings> cfg)
        {
            var settings = cfg.Value;
            var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }

        public async Task<(bool Success, string Url, string PublicId, string Error)> UploadImageAsync(IFormFile file, string folder = "products")
        {
            if (file == null || file.Length == 0)
                return (false, string.Empty, string.Empty, "File is empty");

            // validate content type & size in caller ideally
            try
            {
                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = folder,
                    Transformation = new Transformation().Width(1000).Crop("limit").Quality("auto"),
                    UseFilename = true,
                    UniqueFilename = true,
                    Overwrite = false
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK || uploadResult.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    return (true, uploadResult.SecureUrl.ToString(), uploadResult.PublicId, null);
                }

                return (false, string.Empty, string.Empty, uploadResult.Error?.Message ?? "Upload failed");
            }
            catch (Exception ex)
            {
                return (false, string.Empty, string.Empty, ex.Message);
            }
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId)) return false;
            var deletionParams = new DeletionParams(publicId) { ResourceType = ResourceType.Image };
            var result = await _cloudinary.DestroyAsync(deletionParams);
            return result.Result == "ok" || result.Result == "not_found";
        }
    }
}
